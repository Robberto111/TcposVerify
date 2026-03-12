
using Microsoft.AspNetCore.Mvc;

namespace TcposVerify.Controllers;


public partial class HomeController : Controller
{
	public IActionResult Index() => View();
}
