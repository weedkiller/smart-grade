using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using FirestormSW.SmartGrade.Resources.Localization;

namespace FirestormSW.SmartGrade.Services
{
    public class LanguageService
    {
        public IEnumerable<CultureInfo> GetAvailableCultures()
        {
            var cultures = new List<CultureInfo>();
            var resourceManager = new ResourceManager(typeof(Localization));
            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            foreach (var cultureInfo in allCultures)
            {
                try
                {
                    if (cultureInfo.Equals(CultureInfo.InvariantCulture))
                        continue;
                    var resourceSet = resourceManager.GetResourceSet(cultureInfo, true, false);
                    if (resourceSet != null)
                        cultures.Add(cultureInfo);
                }
                catch (CultureNotFoundException)
                {
                }
            }

            return cultures;
        }
    }
}