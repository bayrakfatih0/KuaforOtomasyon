using Berber.Models;
using Berber.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; 

namespace Berber.Controllers
{
    [Authorize(Roles ="Musteri")]
    public class AIController : Controller
    {
        private readonly GeminiSchedulingService _geminiService;
        private readonly AppointmentAutomationService _bookingService; 

        public AIController(GeminiSchedulingService geminiService, AppointmentAutomationService bookingService)
        {
            _geminiService = geminiService;
            _bookingService = bookingService;
        }


        [HttpPost]
        public async Task<IActionResult> ProcessRequest(string requestText)
        {
            if (string.IsNullOrEmpty(requestText))
            {
                TempData["ErrorMessage"] = "Lütfen randevu isteğinizi yazın.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _geminiService.ParseRequestAsync(requestText);

            if (!result.BasariliMi)
            {
                TempData["ErrorMessage"] = $"İsteğiniz çözümlenemedi: {result.HataMesaji}";
                return RedirectToAction("Index", "Home");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); 

            var (success, message) = await _bookingService.AttemptBookingAsync(result, currentUserId);

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<JsonResult> ProcessLiveRequest(string requestText)
        {
            var aiResult = await _geminiService.ParseRequestAsync(requestText);

            if (!aiResult.BasariliMi)
            {
                return Json(new { message = aiResult.HataMesaji });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var (isBooked, message) = await _bookingService.AttemptBookingAsync(aiResult, userId);

                if (isBooked)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"🎉 Harika! {message}"
                    });
                }
                else
                {
                    return Json(new { message = $"Üzgünüm, {message}. Başka bir saat deneyebilir miyiz?" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { message = "Randevu kaydedilirken teknik bir sorun oluştu." });
            }
        }
    }
}