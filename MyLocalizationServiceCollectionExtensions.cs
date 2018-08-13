//// Copyright (c) .NET Foundation. All rights reserved. 
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Reflection;
//using System.Resources;

//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Localization.Internal;
//using Microsoft.Extensions.Options;

//namespace Microsoft.Extensions.Localization
//{
//    public static class MyLocalizationServiceCollectionExtensions
//    {
//        /// <summary>
//        /// Adds services required for application localization.
//        /// </summary>
//        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
//        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
//        public static IServiceCollection AddMyLocalization(this IServiceCollection services)
//        {
//            if (services == null)
//            {
//                throw new ArgumentNullException(nameof(services));
//            }

//            services.AddOptions();

//            AddLocalizationServices(services);

//            return services;
//        }

//        /// <summary>
//        /// Adds services required for application localization.
//        /// </summary>
//        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
//        /// <param name="setupAction">
//        /// An <see cref="Action{LocalizationOptions}"/> to configure the <see cref="LocalizationOptions"/>.
//        /// </param>
//        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
//        public static IServiceCollection AddMyLocalization(
//            this IServiceCollection services,
//            Action<LocalizationOptions> setupAction)
//        {
//            if (services == null)
//            {
//                throw new ArgumentNullException(nameof(services));
//            }

//            if (setupAction == null)
//            {
//                throw new ArgumentNullException(nameof(setupAction));
//            }

//            AddLocalizationServices(services, setupAction);

//            return services;
//        }

//        // To enable unit testing
//        internal static void AddLocalizationServices(IServiceCollection services)
//        {
//            services.AddSingleton<IStringLocalizerFactory, ResourceManagerStringLocalizer2Factory>();
//            services.AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
//        }

//        internal static void AddLocalizationServices(
//            IServiceCollection services,
//            Action<LocalizationOptions> setupAction)
//        {
//            AddLocalizationServices(services);
//            services.Configure(setupAction);
//        }
//    }

//    /// <summary>
//    /// An <see cref="IStringLocalizerFactory"/> that creates instances of <see cref="ResourceManagerStringLocalizer2"/>.
//    /// </summary>
//    public class ResourceManagerStringLocalizer2Factory : IStringLocalizerFactory
//    {
//        private readonly IResourceNamesCache _resourceNamesCache = new ResourceNamesCache();
//        private readonly ConcurrentDictionary<string, ResourceManagerStringLocalizer2> _localizerCache =
//            new ConcurrentDictionary<string, ResourceManagerStringLocalizer2>();
//        private readonly IHostingEnvironment _hostingEnvironment;
//        private readonly string _resourcesRelativePath;

//        /// <summary>
//        /// Creates a new <see cref="ResourceManagerStringLocalizer2"/>.
//        /// </summary>
//        /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment"/>.</param>
//        /// <param name="localizationOptions">The <see cref="IOptions{LocalizationOptions}"/>.</param>
//        public ResourceManagerStringLocalizer2Factory(
//            IHostingEnvironment hostingEnvironment,
//            IOptions<LocalizationOptions> localizationOptions)
//        {
//            if (hostingEnvironment == null)
//            {
//                throw new ArgumentNullException(nameof(hostingEnvironment));
//            }

//            if (localizationOptions == null)
//            {
//                throw new ArgumentNullException(nameof(localizationOptions));
//            }

//            _hostingEnvironment = hostingEnvironment;
//            _resourcesRelativePath = localizationOptions.Value.ResourcesPath ?? string.Empty;
//            if (!string.IsNullOrEmpty(_resourcesRelativePath))
//            {
//                _resourcesRelativePath = _resourcesRelativePath.Replace(Path.AltDirectorySeparatorChar, '.')
//                    .Replace(Path.DirectorySeparatorChar, '.') + ".";
//            }
//        }

//        /// <summary>
//        /// Creates a <see cref="ResourceManagerStringLocalizer2"/> using the <see cref="Assembly"/> and
//        /// <see cref="Type.FullName"/> of the specified <see cref="Type"/>.
//        /// </summary>
//        /// <param name="resourceSource">The <see cref="Type"/>.</param>
//        /// <returns>The <see cref="ResourceManagerStringLocalizer2"/>.</returns>
//        public IStringLocalizer Create(Type resourceSource)
//        {
//            if (resourceSource == null)
//            {
//                throw new ArgumentNullException(nameof(resourceSource));
//            }

//            var typeInfo = resourceSource.GetTypeInfo();
//            var assembly = typeInfo.Assembly;

//            // Re-root the base name if a resources path is set
//            var baseName = string.IsNullOrEmpty(_resourcesRelativePath)
//                ? typeInfo.FullName
//                : _hostingEnvironment.ApplicationName + "." + _resourcesRelativePath
//                    + TrimPrefix(typeInfo.FullName, _hostingEnvironment.ApplicationName + ".");

//            return _localizerCache.GetOrAdd(baseName, _ =>
//                new ResourceManagerStringLocalizer2(
//                    new ResourceManager(baseName, assembly),
//                    assembly,
//                    baseName,
//                    _resourceNamesCache)
//            );
//        }

//        /// <summary>
//        /// Creates a <see cref="ResourceManagerStringLocalizer2"/>.
//        /// </summary>
//        /// <param name="baseName">The base name of the resource to load strings from.</param>
//        /// <param name="location">The location to load resources from.</param>
//        /// <returns>The <see cref="ResourceManagerStringLocalizer2"/>.</returns>
//        public IStringLocalizer Create(string baseName, string location)
//        {
//            if (baseName == null)
//            {
//                throw new ArgumentNullException(nameof(baseName));
//            }

//            location = location ?? _hostingEnvironment.ApplicationName;

//            baseName = location + "." + _resourcesRelativePath + TrimPrefix(baseName, location + ".");

//            return _localizerCache.GetOrAdd($"B={baseName},L={location}", _ =>
//            {
//                var assembly = Assembly.Load(new AssemblyName(location));
//                return new ResourceManagerStringLocalizer2(
//                    new ResourceManager(baseName, assembly),
//                    assembly,
//                    baseName,
//                    _resourceNamesCache);
//            });
//        }

//        private static string TrimPrefix(string name, string prefix)
//        {
//            if (name.StartsWith(prefix, StringComparison.Ordinal))
//            {
//                return name.Substring(prefix.Length);
//            }

//            return name;
//        }
//    }


//    /// <summary>
//    /// An <see cref="IStringLocalizer"/> that uses the <see cref="ResourceManager"/> and
//    /// <see cref="ResourceReader"/> to provide localized strings.
//    /// </summary>
//    /// <remarks>This type is thread-safe.</remarks>
//    public class ResourceManagerStringLocalizer2 : IStringLocalizer
//    {
//        private readonly ConcurrentDictionary<string, object> _missingManifestCache = new ConcurrentDictionary<string, object>();
//        private readonly IResourceNamesCache _resourceNamesCache;
//        private readonly ResourceManager _resourceManager;
//        private readonly IResourceStringProvider _resourceStringProvider;
//        private readonly string _resourceBaseName;

//        /// <summary>
//        /// Creates a new <see cref="ResourceManagerStringLocalizer2"/>.
//        /// </summary>
//        /// <param name="resourceManager">The <see cref="ResourceManager"/> to read strings from.</param>
//        /// <param name="resourceAssembly">The <see cref="Assembly"/> that contains the strings as embedded resources.</param>
//        /// <param name="baseName">The base name of the embedded resource that contains the strings.</param>
//        /// <param name="resourceNamesCache">Cache of the list of strings for a given resource assembly name.</param>
//        public ResourceManagerStringLocalizer2(
//            ResourceManager resourceManager,
//            Assembly resourceAssembly,
//            string baseName,
//            IResourceNamesCache resourceNamesCache)
//            : this(
//                resourceManager,
//                new AssemblyWrapper(resourceAssembly),
//                baseName,
//                resourceNamesCache)
//        {
//        }

//        /// <summary>
//        /// Intended for testing purposes only.
//        /// </summary>
//        public ResourceManagerStringLocalizer2(
//            ResourceManager resourceManager,
//            AssemblyWrapper resourceAssemblyWrapper,
//            string baseName,
//            IResourceNamesCache resourceNamesCache)
//            : this(
//                  resourceManager,
//                  new AssemblyResourceStringProvider(resourceNamesCache, resourceAssemblyWrapper, baseName),
//                  baseName,
//                  resourceNamesCache)
//        {
//        }

//        /// <summary>
//        /// Intended for testing purposes only.
//        /// </summary>
//        public ResourceManagerStringLocalizer2(
//            ResourceManager resourceManager,
//            IResourceStringProvider resourceStringProvider,
//            string baseName,
//            IResourceNamesCache resourceNamesCache)
//        {
//            if (resourceManager == null)
//            {
//                throw new ArgumentNullException(nameof(resourceManager));
//            }

//            if (resourceStringProvider == null)
//            {
//                throw new ArgumentNullException(nameof(resourceStringProvider));
//            }

//            if (baseName == null)
//            {
//                throw new ArgumentNullException(nameof(baseName));
//            }

//            if (resourceNamesCache == null)
//            {
//                throw new ArgumentNullException(nameof(resourceNamesCache));
//            }

//            _resourceStringProvider = resourceStringProvider;
//            _resourceManager = resourceManager;
//            _resourceBaseName = baseName;
//            _resourceNamesCache = resourceNamesCache;
//        }

//        /// <inheritdoc />
//        public virtual LocalizedString this[string name]
//        {
//            get
//            {
//                if (name == null)
//                {
//                    throw new ArgumentNullException(nameof(name));
//                }

//                var value = GetStringSafely(name, null);

//                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
//            }
//        }

//        /// <inheritdoc />
//        public virtual LocalizedString this[string name, params object[] arguments]
//        {
//            get
//            {
//                if (name == null)
//                {
//                    throw new ArgumentNullException(nameof(name));
//                }

//                var format = GetStringSafely(name, null);
//                var value = string.Format(format ?? name, arguments);

//                return new LocalizedString(name, value, resourceNotFound: format == null);
//            }
//        }

//        /// <summary>
//        /// Creates a new <see cref="ResourceManagerStringLocalizer2"/> for a specific <see cref="CultureInfo"/>.
//        /// </summary>
//        /// <param name="culture">The <see cref="CultureInfo"/> to use.</param>
//        /// <returns>A culture-specific <see cref="ResourceManagerStringLocalizer2"/>.</returns>
//        public IStringLocalizer WithCulture(CultureInfo culture)
//        {
//            return culture == null
//                ? new ResourceManagerStringLocalizer2(
//                    _resourceManager,
//                    _resourceStringProvider,
//                    _resourceBaseName,
//                    _resourceNamesCache)
//                : new ResourceManagerWithCultureStringLocalizer2(
//                    _resourceManager,
//                    _resourceStringProvider,
//                    _resourceBaseName,
//                    _resourceNamesCache,
//                    culture);
//        }

//        /// <inheritdoc />
//        public virtual IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
//            GetAllStrings(includeParentCultures, CultureInfo.CurrentUICulture);

//        /// <summary>
//        /// Returns all strings in the specified culture.
//        /// </summary>
//        /// <param name="includeParentCultures"></param>
//        /// <param name="culture">The <see cref="CultureInfo"/> to get strings for.</param>
//        /// <returns>The strings.</returns>
//        protected IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures, CultureInfo culture)
//        {
//            if (culture == null)
//            {
//                throw new ArgumentNullException(nameof(culture));
//            }

//            var resourceNames = includeParentCultures
//                ? GetResourceNamesFromCultureHierarchy(culture)
//                : _resourceStringProvider.GetAllResourceStrings(culture, true);

//            foreach (var name in resourceNames)
//            {
//                var value = GetStringSafely(name, culture);
//                yield return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
//            }
//        }

//        /// <summary>
//        /// Gets a resource string from the <see cref="_resourceManager"/> and returns <c>null</c> instead of
//        /// throwing exceptions if a match isn't found.
//        /// </summary>
//        /// <param name="name">The name of the string resource.</param>
//        /// <param name="culture">The <see cref="CultureInfo"/> to get the string for.</param>
//        /// <returns>The resource string, or <c>null</c> if none was found.</returns>
//        protected string GetStringSafely(string name, CultureInfo culture)
//        {
//            if (name == null)
//            {
//                throw new ArgumentNullException(nameof(name));
//            }

//            var cacheKey = $"name={name}&culture={(culture ?? CultureInfo.CurrentUICulture).Name}";

//            if (_missingManifestCache.ContainsKey(cacheKey))
//            {
//                return null;
//            }

//            try
//            {
//                return culture == null ? _resourceManager.GetString(name) : _resourceManager.GetString(name, culture);
//            }
//            catch (MissingManifestResourceException)
//            {
//                _missingManifestCache.TryAdd(cacheKey, null);
//                return null;
//            }
//        }

//        private IEnumerable<string> GetResourceNamesFromCultureHierarchy(CultureInfo startingCulture)
//        {
//            var currentCulture = startingCulture;
//            var resourceNames = new HashSet<string>();

//            var hasAnyCultures = false;

//            while (true)
//            {

//                var cultureResourceNames = _resourceStringProvider.GetAllResourceStrings(currentCulture, false);

//                if (cultureResourceNames != null)
//                {
//                    foreach (var resourceName in cultureResourceNames)
//                    {
//                        resourceNames.Add(resourceName);
//                    }
//                    hasAnyCultures = true;
//                }

//                if (currentCulture == currentCulture.Parent)
//                {
//                    // currentCulture begat currentCulture, probably time to leave
//                    break;
//                }

//                currentCulture = currentCulture.Parent;
//            }

//            if (!hasAnyCultures)
//            {
//                throw new MissingManifestResourceException("sdfsdf");
//            }

//            return resourceNames;
//        }
//    }

//    /// <summary>
//    /// An <see cref="IStringLocalizer"/> that uses the <see cref="ResourceManager"/> and
//    /// <see cref="ResourceReader"/> to provide localized strings for a specific <see cref="CultureInfo"/>.
//    /// </summary>
//    public class ResourceManagerWithCultureStringLocalizer2 : ResourceManagerStringLocalizer2
//    {
//        private readonly CultureInfo _culture;

//        /// <summary>
//        /// Creates a new <see cref="ResourceManagerWithCultureStringLocalizer2"/>.
//        /// </summary>
//        /// <param name="resourceManager">The <see cref="ResourceManager"/> to read strings from.</param>
//        /// <param name="resourceStringProvider">The <see cref="IResourceStringProvider"/> that can find the resources.</param>
//        /// <param name="baseName">The base name of the embedded resource that contains the strings.</param>
//        /// <param name="resourceNamesCache">Cache of the list of strings for a given resource assembly name.</param>
//        /// <param name="culture">The specific <see cref="CultureInfo"/> to use.</param>
//        internal ResourceManagerWithCultureStringLocalizer2(
//            ResourceManager resourceManager,
//            IResourceStringProvider resourceStringProvider,
//            string baseName,
//            IResourceNamesCache resourceNamesCache,
//            CultureInfo culture)
//            : base(resourceManager, resourceStringProvider, baseName, resourceNamesCache)
//        {
//            if (resourceManager == null)
//            {
//                throw new ArgumentNullException(nameof(resourceManager));
//            }

//            if (resourceStringProvider == null)
//            {
//                throw new ArgumentNullException(nameof(resourceStringProvider));
//            }

//            if (baseName == null)
//            {
//                throw new ArgumentNullException(nameof(baseName));
//            }

//            if (resourceNamesCache == null)
//            {
//                throw new ArgumentNullException(nameof(resourceNamesCache));
//            }

//            if (culture == null)
//            {
//                throw new ArgumentNullException(nameof(culture));
//            }

//            _culture = culture;
//        }

//        /// <summary>
//        /// Creates a new <see cref="ResourceManagerWithCultureStringLocalizer2"/>.
//        /// </summary>
//        /// <param name="resourceManager">The <see cref="ResourceManager"/> to read strings from.</param>
//        /// <param name="resourceAssembly">The <see cref="Assembly"/> that contains the strings as embedded resources.</param>
//        /// <param name="baseName">The base name of the embedded resource that contains the strings.</param>
//        /// <param name="resourceNamesCache">Cache of the list of strings for a given resource assembly name.</param>
//        /// <param name="culture">The specific <see cref="CultureInfo"/> to use.</param>
//        public ResourceManagerWithCultureStringLocalizer2(
//            ResourceManager resourceManager,
//            Assembly resourceAssembly,
//            string baseName,
//            IResourceNamesCache resourceNamesCache,
//            CultureInfo culture)
//            : base(resourceManager, resourceAssembly, baseName, resourceNamesCache)
//        {
//            if (resourceManager == null)
//            {
//                throw new ArgumentNullException(nameof(resourceManager));
//            }

//            if (resourceAssembly == null)
//            {
//                throw new ArgumentNullException(nameof(resourceAssembly));
//            }

//            if (baseName == null)
//            {
//                throw new ArgumentNullException(nameof(baseName));
//            }

//            if (resourceNamesCache == null)
//            {
//                throw new ArgumentNullException(nameof(resourceNamesCache));
//            }

//            if (culture == null)
//            {
//                throw new ArgumentNullException(nameof(culture));
//            }

//            _culture = culture;
//        }

//        /// <inheritdoc />
//        public override LocalizedString this[string name]
//        {
//            get
//            {
//                if (name == null)
//                {
//                    throw new ArgumentNullException(nameof(name));
//                }

//                var value = GetStringSafely(name, _culture);

//                return new LocalizedString(name, value ?? name);
//            }
//        }

//        /// <inheritdoc />
//        public override LocalizedString this[string name, params object[] arguments]
//        {
//            get
//            {
//                if (name == null)
//                {
//                    throw new ArgumentNullException(nameof(name));
//                }

//                var format = GetStringSafely(name, _culture);
//                var value = string.Format(_culture, format ?? name, arguments);

//                return new LocalizedString(name, value ?? name, resourceNotFound: format == null);
//            }
//        }

//        /// <inheritdoc />
//        public override IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
//            GetAllStrings(includeParentCultures, _culture);
//    }
//}