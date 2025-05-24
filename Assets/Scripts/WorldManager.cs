using UnityEngine;

namespace Resilience.World
{
    /// <summary>
    /// Manages the game world, including day/night cycle and weather systems
    /// </summary>
    public class WorldManager : MonoBehaviour
    {
        [Header("Day/Night Cycle")]
        [SerializeField] private Light directionalLight;
        [SerializeField] private float dayDuration = 20f; // Duration of a full day in minutes
        [SerializeField] private Color dayAmbientColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color nightAmbientColor = new Color(0.1f, 0.1f, 0.2f);
        
        [Header("Weather System")]
        [SerializeField] private ParticleSystem rainSystem;
        [SerializeField] private ParticleSystem snowSystem;
        [SerializeField] private AudioSource windAudioSource;
        [SerializeField] private AudioSource rainAudioSource;
        [SerializeField] private float weatherChangeInterval = 300f; // Time between weather changes in seconds
        
        // Private variables
        private float currentTime = 0f; // Current time of day in seconds
        private float dayLengthInSeconds;
        private float weatherTimer = 0f;
        private WeatherType currentWeather = WeatherType.Clear;
        
        private enum WeatherType
        {
            Clear,
            Cloudy,
            Rainy,
            Stormy,
            Snowy
        }
        
        private void Start()
        {
            dayLengthInSeconds = dayDuration * 60f;
            
            // Initialize weather systems
            if (rainSystem != null)
                rainSystem.Stop();
            if (snowSystem != null)
                snowSystem.Stop();
                
            // Start with a random time of day
            currentTime = Random.Range(0f, dayLengthInSeconds);
            
            UpdateDayNightCycle();
        }
        
        private void Update()
        {
            // Update day/night cycle
            currentTime += Time.deltaTime;
            if (currentTime > dayLengthInSeconds)
            {
                currentTime -= dayLengthInSeconds;
            }
            
            UpdateDayNightCycle();
            
            // Update weather system
            weatherTimer += Time.deltaTime;
            if (weatherTimer >= weatherChangeInterval)
            {
                weatherTimer = 0f;
                ChangeWeather();
            }
        }
        
        /// <summary>
        /// Updates the day/night cycle based on current time
        /// </summary>
        private void UpdateDayNightCycle()
        {
            // Calculate sun rotation
            float timeRatio = currentTime / dayLengthInSeconds;
            float sunAngle = timeRatio * 360f;
            
            directionalLight.transform.rotation = Quaternion.Euler(sunAngle - 90f, 170f, 0f);
            
            // Calculate ambient light based on time of day
            float dayNightRatio = Mathf.PingPong(timeRatio * 2f, 1f);
            RenderSettings.ambientLight = Color.Lerp(nightAmbientColor, dayAmbientColor, dayNightRatio);
            
            // Adjust light intensity
            float lightIntensity = Mathf.Clamp(dayNightRatio * 1.5f, 0.1f, 1.0f);
            directionalLight.intensity = lightIntensity;
        }
        
        /// <summary>
        /// Changes the current weather randomly
        /// </summary>
        private void ChangeWeather()
        {
            // Stop current weather effects
            StopCurrentWeather();
            
            // Randomly select new weather
            int weatherChance = Random.Range(0, 100);
            
            if (weatherChance < 50)
            {
                currentWeather = WeatherType.Clear;
            }
            else if (weatherChance < 70)
            {
                currentWeather = WeatherType.Cloudy;
            }
            else if (weatherChance < 85)
            {
                currentWeather = WeatherType.Rainy;
            }
            else if (weatherChance < 95)
            {
                currentWeather = WeatherType.Stormy;
            }
            else
            {
                currentWeather = WeatherType.Snowy;
            }
            
            // Apply new weather effects
            ApplyWeatherEffects();
        }
        
        /// <summary>
        /// Stops all current weather effects
        /// </summary>
        private void StopCurrentWeather()
        {
            if (rainSystem != null)
                rainSystem.Stop();
            if (snowSystem != null)
                snowSystem.Stop();
            if (rainAudioSource != null)
                rainAudioSource.Stop();
            if (windAudioSource != null)
                windAudioSource.volume = 0.2f;
        }
        
        /// <summary>
        /// Applies weather effects based on current weather type
        /// </summary>
        private void ApplyWeatherEffects()
        {
            switch (currentWeather)
            {
                case WeatherType.Clear:
                    // No special effects for clear weather
                    if (windAudioSource != null)
                        windAudioSource.volume = 0.1f;
                    break;
                    
                case WeatherType.Cloudy:
                    if (windAudioSource != null)
                        windAudioSource.volume = 0.3f;
                    break;
                    
                case WeatherType.Rainy:
                    if (rainSystem != null)
                        rainSystem.Play();
                    if (rainAudioSource != null)
                        rainAudioSource.Play();
                    if (windAudioSource != null)
                        windAudioSource.volume = 0.4f;
                    break;
                    
                case WeatherType.Stormy:
                    if (rainSystem != null)
                    {
                        var emission = rainSystem.emission;
                        emission.rateOverTime = 500f;
                        rainSystem.Play();
                    }
                    if (rainAudioSource != null)
                    {
                        rainAudioSource.volume = 1.0f;
                        rainAudioSource.Play();
                    }
                    if (windAudioSource != null)
                        windAudioSource.volume = 0.8f;
                    break;
                    
                case WeatherType.Snowy:
                    if (snowSystem != null)
                        snowSystem.Play();
                    if (windAudioSource != null)
                        windAudioSource.volume = 0.5f;
                    break;
            }
            
            Debug.Log($"Weather changed to: {currentWeather}");
        }
        
        /// <summary>
        /// Gets the current time of day as a normalized value (0-1)
        /// </summary>
        /// <returns>Normalized time of day</returns>
        public float GetNormalizedTimeOfDay()
        {
            return currentTime / dayLengthInSeconds;
        }
        
        /// <summary>
        /// Gets the current weather type
        /// </summary>
        /// <returns>Current weather as string</returns>
        public string GetCurrentWeather()
        {
            return currentWeather.ToString();
        }
        
        /// <summary>
        /// Checks if it's currently daytime
        /// </summary>
        /// <returns>True if it's daytime</returns>
        public bool IsDaytime()
        {
            float normalizedTime = GetNormalizedTimeOfDay();
            return normalizedTime > 0.25f && normalizedTime < 0.75f;
        }
    }
}
