using System.Text.Json;
using TMDBLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TMDBLibrary.Services
{
    public class TMDBService
    {
        private readonly string _apiKey = "cbd5255ed028c3327219c80d31dff7c7";  // Consider moving this to configuration.
        private const string BaseUrl = "https://api.themoviedb.org/3/";
        private readonly HttpClient _client;
        private readonly ILogger<TMDBService> _logger;

        public TMDBService(HttpClient client, ILogger<TMDBService> logger)
        {
            _client = client;
            _logger = logger;
            _client.BaseAddress = new Uri(BaseUrl);
        }

        public async Task<IEnumerable<Movie>> GetPopularMovies()
        {
            var response = await _client.GetAsync($"movie/popular?api_key={_apiKey}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"TMDB API returned status code: {response.StatusCode}");
                throw new HttpRequestException($"TMDB API returned status code: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(content);  // Be careful with logging full content, can flood your logs.

            var tmdbResponse = JsonSerializer.Deserialize<TMDBResponse>(content);

            if (tmdbResponse?.Results == null)
            {
                _logger.LogWarning("No results from TMDB API.");
                return Enumerable.Empty<Movie>();
            }

            var movies = tmdbResponse.Results.Select(m => new Movie
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Overview,
                ReleaseDate = m.ReleaseDate
            }).ToList();

            _logger.LogInformation($"{movies.Count} movies processed.");
            return movies;
        }
    }

    // Local classes to help deserialize TMDB's response
    public class TMDBResponse
    {
        public int Page { get; set; }
        public List<TMDBMovie> Results { get; set; }
    }

    public class TMDBMovie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}
