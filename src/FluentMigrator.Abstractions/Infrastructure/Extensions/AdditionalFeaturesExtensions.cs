using System;

namespace FluentMigrator.Infrastructure.Extensions
{
    public static class AdditionalFeaturesExtensions
    {
        public static T GetAdditionalFeature<T>(this ISupportAdditionalFeatures additionalFeatures, string key, T defaultValue = default)
        {
            T result;
            var dict = additionalFeatures.AdditionalFeatures;
            if (!dict.TryGetValue(key, out var val))
            {
                result = defaultValue;
                dict.Add(key, result);
            }
            else
            {
                result = (T)val;
            }

            return result;
        }

        public static T GetAdditionalFeature<T>(this ISupportAdditionalFeatures additionalFeatures, string key, Func<T> createDefaultValue)
        {
            T result;
            var dict = additionalFeatures.AdditionalFeatures;
            if (!dict.TryGetValue(key, out var val))
            {
                result = createDefaultValue();
                dict.Add(key, result);
            }
            else
            {
                result = (T)val;
            }

            return result;
        }

        public static void SetAdditionalFeature<T>(this ISupportAdditionalFeatures additionalFeatures, string key, T value)
        {
            var dict = additionalFeatures.AdditionalFeatures;
            dict[key] = value;
        }
    }
}
