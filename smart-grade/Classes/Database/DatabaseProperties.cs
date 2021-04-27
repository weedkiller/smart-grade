using System.Linq;
using FirestormSW.SmartGrade.Database.Model;
using Newtonsoft.Json.Linq;

namespace FirestormSW.SmartGrade.Database
{
    public class DatabaseProperties
    {
        private readonly AppDatabase database;

        public DatabaseProperties(AppDatabase database)
        {
            this.database = database;
        }

        public void SetProperty<T>(string key, T value) where T : class, new()
        {
            var data = JObject.FromObject(value);
            var property = database.Properties.SingleOrDefault(p => p.Key == key)
                           ?? new Property {Key = key};
            property.Value = data.ToString();

            if (!database.Properties.Contains(property))
                database.Properties.Add(property);
            database.SaveChanges();
        }

        public void SetProperty<T>(T value) where T : class, new()
            => SetProperty(typeof(T).Name, value);

        public T GetProperty<T>(string key) where T : class, new()
        {
            var property = database.Properties.SingleOrDefault(p => p.Key == key);
            if (property == null)
                return null;

            return JObject.Parse(property.Value).ToObject<T>();
        }

        public T GetProperty<T>() where T : class, new()
            => GetProperty<T>(typeof(T).Name);

        public T GetOrCreateProperty<T>() where T : class, new()
        {
            var prop = GetProperty<T>() ?? new T();
            SetProperty(prop);
            return prop;
        }
    }
}