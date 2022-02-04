using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  // derive AspNetCore.Mvc's Controller, angular build file is view of MVC
  public class FallbackController : Controller
  {
    // load index.html file
    public ActionResult Index()
    {
      return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(),
         "wwwroot", "index.html"), "text/HTML");
    }
  }
}