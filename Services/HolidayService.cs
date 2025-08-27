using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ClassroomReservationSystem.Services
{
    public class HolidayService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;

        public HolidayService(IHttpClientFactory clientFactory, IConfiguration config)
        {
            _clientFactory = clientFactory;
            _config = config;
        }

        public async Task<bool> IsHoliday(DateTime date)
        {
            try
            {
                var client = _clientFactory.CreateClient("GoogleCalendar");
                var response = await client.GetAsync(
                    $"calendars/{Uri.EscapeDataString(_config["GoogleCalendar:CalendarId"])}/events?" +
                    $"key={_config["GoogleCalendar:ApiKey"]}&" +
                    $"timeMin={date:yyyy-MM-dd}&timeMax={date.AddDays(1):yyyy-MM-dd}&singleEvents=true");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return content.Contains("\"summary\":");
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<DateTime>> GetHolidaysInRange(DateTime start, DateTime end)
        {
            try
            {
                var client = _clientFactory.CreateClient("GoogleCalendar");
                var calendarId = "tr.turkish#holiday@group.v.calendar.google.com"; // Turkish holidays
                var apiKey = _config["GoogleCalendar:ApiKey"];

                var requestUrl = $"calendars/{Uri.EscapeDataString(calendarId)}/events?" +
                    $"key={apiKey}&" +
                    $"timeMin={start:yyyy-MM-dd}T00:00:00Z&" +
                    $"timeMax={end:yyyy-MM-dd}T23:59:59Z&" +
                    $"singleEvents=true";

                var response = await client.GetAsync(requestUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var calendarResponse = JsonSerializer.Deserialize<GoogleCalendarResponse>(content, options);
                    return calendarResponse?.Items
                        .Where(e => e.Start?.Date != null)
                        .Select(e => DateTime.Parse(e.Start.Date))
                        .ToList() ?? new List<DateTime>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Holiday API Error: {ex.Message}");
            }
            
            return new List<DateTime>();
        }

        private class GoogleCalendarResponse
        {
            public List<CalendarEvent> Items { get; set; } = new();
        }

        private class CalendarEvent
        {
            public EventDateTime Start { get; set; }
            public string Summary { get; set; }
        }

        private class EventDateTime
        {
            public string Date { get; set; }
        }
    }
}