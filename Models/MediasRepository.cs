using DAL;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    public class MediasRepository : Repository<Media>
    {
        public List<string> MediasCategories()
        {
            List<string> Categories = new List<string>();
            bool userConnected = Models.User.ConnectedUser != null;
            bool userIsAdmin = userConnected ? Models.User.ConnectedUser.IsAdmin : false;
            int currentUserId = Models.User.ConnectedUser.Id;

            IEnumerable<Media> medias = ToList();

            if (!userIsAdmin)
                medias = medias.Where(m => m.Shared || m.OwnerId == currentUserId);

            foreach (Media media in medias.OrderBy(m => m.Category))
            {
                if (!Categories.Contains(media.Category))
                {
                    Categories.Add(media.Category);
                }
            }

            return Categories;
        }
    }
}