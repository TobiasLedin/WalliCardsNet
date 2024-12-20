﻿using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using WalliCardsNet.API.Data.Interfaces;

namespace WalliCardsNet.API.Controllers
{
    [Route("/api/payment")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly IBusiness _businessRepo;
        public StripeController(IBusiness businessRepo)
        {
            _businessRepo = businessRepo;
        }


        [HttpPost("create-checkout-session")]
        public IActionResult CreateCheckoutSession() //TODO: Fix input-validation of email
        {
            var domain = "https://localhost:7102/";

            var customerEmail = Request.Form["customer_email"];
            var priceOptions = new PriceListOptions
            {
                LookupKeys = new List<string>
                { 
                    Request.Form["lookup-key"] 
                }
            };

            var priceService = new PriceService();
            StripeList<Price> prices = priceService.List(priceOptions);

            if (prices.Data == null || prices.Data.Count == 0)
            {
                return BadRequest("No prices found for the provided lookup key.");
            }

            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = prices.Data[0].Id,
                        Quantity = 1
                    },
                },
                Mode = "subscription",
                SuccessUrl = domain + "success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + "cancel",
                CustomerEmail = customerEmail
            };

            var service = new SessionService();
            Session session = service.Create(options);


            //Response.Headers.Add("Location", session.Url);
            //return new StatusCodeResult(303);
            return Ok(new { url = session.Url });
        }
    }
}
