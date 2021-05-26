using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class LanguageService : IDisposable
    {

        

        

        #region Methods 
        public virtual void DeleteLanguage(Language language)
        {
            if (language == null)
                throw new ArgumentNullException("language"); 
            var role = db.Languages.Attach(language);
            db.Set<Language>().Remove(role);
            db.SaveChanges(); 
        }
 
        public virtual IList<Language> GetAllLanguages(bool showHidden = false )
        {
            
            var query = from x in db.Languages select x;
            if (!showHidden)
                query = query.Where(l => l.Published!=null && l.Published.Value);
            query = query.OrderBy(l => l.Order); 
            var languages = query.ToList();
            return languages;
            
        }

        
        public virtual int GetLanguagesCount(bool showHidden = false)
        {

                var query = from x in db.Languages select x;
                if (!showHidden)
                query = query.Where(l => l.Published != null && l.Published.Value);
                return query.Select(x => x.ID).Count();
            
        }

        
        public virtual Language GetLanguageById(string languageId)
        {
            if (languageId.Length<1)
                return null; 
            return db.Languages.Find(languageId);
        }

       
        

        #endregion

        private WebContext db = new WebContext();
        public void Dispose()
        {
            db.Dispose(); 
        }
    }
}