﻿using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebClient.Pages;

public class HelloModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HelloModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    [BindProperty]
    public OutputModel Output { get; set; }

    public async Task<IActionResult> OnGetAsync(string username)
    {
        using var client = _httpClientFactory.CreateClient();

        var greetingResponse = await client.GetAsync($"https://localhost:5003/hello?username={HttpUtility.UrlEncode(username)}");

        Output = await JsonSerializer.DeserializeAsync<OutputModel>(
            await greetingResponse.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return Page();
    }

    public record OutputModel(string Greeting);
}
