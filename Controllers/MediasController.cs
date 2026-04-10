using DAL;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using static Controllers.AccessControl;

[UserAccess(Access.View)]
public class MediasController : Controller
{

    private void InitSessionVariables()
    {
        // Session is a dictionary that hold keys values specific to a session
        // Each user of this web application have their own Session
        // A Session has a default time out of 20 minutes, after time out it is cleared

        if (Session["CurrentMediaId"] == null) Session["CurrentMediaId"] = 0;
        if (Session["CurrentMediaTitle"] == null) Session["CurrentMediaTitle"] = "";
        if (Session["Search"] == null) Session["Search"] = false;
        if (Session["SearchString"] == null) Session["SearchString"] = "";
        if (Session["SelectedCategory"] == null) Session["SelectedCategory"] = "";
        if (Session["Categories"] == null) Session["Categories"] = DB.Medias.MediasCategories();
        if (Session["SortByTitle"] == null) Session["SortByTitle"] = true;
        if (Session["SortAscending"] == null) Session["SortAscending"] = true;
        ValidateSelectedCategory();
    }

    private void ResetCurrentMediaInfo()
    {
        Session["CurrentMediaId"] = 0;
        Session["CurrentMediaTitle"] = "";
    }

    private void ValidateSelectedCategory()
    {
        if (Session["SelectedCategory"] != null)
        {
            var selectedCategory = (string)Session["SelectedCategory"];
            var Medias = DB.Medias.ToList().Where(c => c.Category == selectedCategory);
            if (Medias.Count() == 0)
                Session["SelectedCategory"] = "";
        }
    }

    public ActionResult GetMediasCategoriesList(bool forceRefresh = false)
    {
        try
        {
            InitSessionVariables();

            bool search = (bool)Session["Search"];

            if (search)
            {
                return PartialView();
            }
            return null;
        }
        catch (System.Exception ex)
        {
            return Content("Erreur interne" + ex.Message, "text/html");
        }
    }
    // This action produce a partial view of Medias
    // It is meant to be called by an AJAX request (from client script)

    public ActionResult GetMediaDetails(bool forceRefresh = false)
    {
        try
        {
            InitSessionVariables();

            int mediaId = (int)Session["CurrentMediaId"];
            Media Media = DB.Medias.Get(mediaId);

            if (DB.Users.HasChanged || DB.Medias.HasChanged || forceRefresh)
            {
                return PartialView(Media);
            }

            return null;
        }
        catch (System.Exception ex)
        {
            return Content("Erreur interne" + ex.Message, "text/html");
        }
    }
    public ActionResult GetMedias(bool forceRefresh = false)
    {
        try
        {
            IEnumerable<Media> result = null;
            // Must evaluate HasChanged before forceRefresh, this will fix an usefull refresh
            if (/*DB.Users.HasChanged ||*/ DB.Medias.HasChanged || forceRefresh)
            {
                // forceRefresh is true when a related view is produce
                // DB.Medias.HasChanged is true when a change has been applied on any Media

                InitSessionVariables();
                bool search = (bool)Session["Search"];
                string searchString = (string)Session["SearchString"];

                if (search)
                {
                    result = DB.Medias.ToList().Where(c => c.Title.ToLower().Contains(searchString)).OrderBy(c => c.Title);
                    string SelectedCategory = (string)Session["SelectedCategory"];
                    if (SelectedCategory != "")
                        result = result.Where(c => c.Category == SelectedCategory);
                }
                else
                    result = DB.Medias.ToList();
                if ((bool)Session["SortAscending"])
                {
                    if ((bool)Session["SortByTitle"])
                        result = result.OrderBy(c => c.Title);
                    else
                        result = result.OrderBy(c => c.PublishDate);
                }
                else
                {
                    if ((bool)Session["SortByTitle"])
                        result = result.OrderByDescending(c => c.Title);
                    else
                        result = result.OrderByDescending(c => c.PublishDate);
                }
                return PartialView(result);
            }
            return null;
        }
        catch (System.Exception ex)
        {
            return Content("Erreur interne" + ex.Message, "text/html");
        }
    }


    public ActionResult List()
    {
        ViewBag.PageTitle = "Wikimédia";
        ResetCurrentMediaInfo();
        return View();
    }

    public ActionResult ToggleSearch()
    {
        if (Session["Search"] == null) Session["Search"] = false;
        Session["Search"] = !(bool)Session["Search"];
        return RedirectToAction("List");
    }
    public ActionResult SortByTitle()
    {
        Session["SortByTitle"] = true;
        return RedirectToAction("List");
    }
    public ActionResult ToggleSort()
    {
        Session["SortAscending"] = !(bool)Session["SortAscending"];
        return RedirectToAction("List");
    }
    public ActionResult SortByDate()
    {
        Session["SortByTitle"] = false;
        return RedirectToAction("List");
    }

    public ActionResult SetSearchString(string value)
    {
        Session["SearchString"] = value.ToLower();
        return RedirectToAction("List");
    }

    public ActionResult SetSearchCategory(string value)
    {
        Session["SelectedCategory"] = value;
        return RedirectToAction("List");
    }
    public ActionResult About()
    {
        return View();
    }


    public ActionResult Details(int id)
    {
        Session["CurrentMediaId"] = id;
        Media Media = DB.Medias.Get(id);
        if (Media != null)
        {
            Session["CurrentMediaTitle"] = Media.Title;
            return View(Media);
        }
        return RedirectToAction("List");
    }
    [UserAccess(Access.Write)]
    public ActionResult Create()
    {
        return View(new Media());
    }

    [HttpPost]
    [UserAccess(Access.Write)]
    /* Install anti forgery token verification attribute.
     * the goal is to prevent submission of data from a page 
     * that has not been produced by this application*/
    [ValidateAntiForgeryToken()]
    public ActionResult Create(Media Media)
    {
        Media.OwnerId = Models.User.ConnectedUser.Id;
        DB.Medias.Add(Media);
        return RedirectToAction("List");
    }
    
    [UserAccess(Access.Write)]
    public ActionResult Edit()
    {
        bool userConnected = Models.User.ConnectedUser != null;
        if (!userConnected)
            return RedirectToAction("Login", "Accounts", new { message = "Accès illégal!", success = false });

        bool userIsAdmin = Models.User.ConnectedUser.IsAdmin;
        int currentUserId = Models.User.ConnectedUser.Id;

        int id = Session["CurrentMediaId"] != null ? (int)Session["CurrentMediaId"] : 0;
        Media media = DB.Medias.Get(id);
        if (media == null)
            return RedirectToAction("List");

        int ownerId = media.OwnerId;

        // Correct logic
        if (!userIsAdmin && ownerId != currentUserId)
            return RedirectToAction("Login", "Accounts", new { message = "Accès illégal!", success = false });

        return View(media);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(Media posted)
    {
        int id = Session["CurrentMediaId"] != null ? (int)Session["CurrentMediaId"] : 0;

        Media storedMedia = DB.Medias.Get(id);
        if (storedMedia != null)
        {
            // Update only fields the user is allowed to change
            storedMedia.Title = posted.Title;
            storedMedia.Category = posted.Category;
            storedMedia.Description = posted.Description;
            storedMedia.YoutubeId = posted.YoutubeId;
            storedMedia.Shared = posted.Shared;

            // OwnerId stays untouched
            // PublishDate stays untouched

            DB.Medias.Update(storedMedia);
        }

        return RedirectToAction("Details/" + id);
    }

    [UserAccess(Access.Write)]
    public ActionResult Delete()
    {
        var user = Models.User.ConnectedUser;

        if (user == null)
            return RedirectToAction("Login", "Accounts", new { message = "Accès illégal!", success = false });

        int id = Session["CurrentMediaId"] != null ? (int)Session["CurrentMediaId"] : 0;
        Media media = DB.Medias.Get(id);

        if (media == null)
            return RedirectToAction("List");

        bool userIsAdmin = user.IsAdmin;
        int currentUserId = user.Id;
        int ownerId = media.OwnerId;

        if (ownerId == currentUserId || userIsAdmin)
        {
            DB.Medias.Delete(id);
            return RedirectToAction("List");   // ✔️ correct redirect for authorized users
        }

        return RedirectToAction("Login", "Accounts", new { message = "Accès illégal!", success = false });
    }

    // This action is meant to be called by an AJAX request
    // Return true if there is a name conflict
    // Look into validation.js for more details
    // and also into Views/Medias/MediaForm.cshtml
    public JsonResult CheckConflict(string YoutubeId)
    {
        int id = Session["CurrentMediaId"] != null ? (int)Session["CurrentMediaId"] : 0;
        // Response json value true if name is used in other Medias than the current Media
        return Json(DB.Medias.ToList().Where(c => c.YoutubeId == YoutubeId && c.Id != id).Any(),
                    JsonRequestBehavior.AllowGet /* must have for CORS verification by client browser */);
    }

}
