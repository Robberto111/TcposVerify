
using Microsoft.AspNetCore.Mvc;
using TcposVerify.Models;
using TcposVerify.Services;

namespace TcposVerify.Controllers;


[Route("api/v1/[controller]")]
public partial class VerifyController : Controller
{
	private readonly IVerifyService m_VerifyService;

	//////////////////////////////////////////////////////////////
	public VerifyController(IVerifyService verifyService)
	{
		m_VerifyService = verifyService;
	}

	//////////////////////////////////////////////////////////////
	[HttpGet]
	public async Task<IActionResult> Index([FromQuery] VerifyRequest req)
	{
		string queryString = HttpContext.Request.QueryString.ToString().Replace("?", string.Empty);

		// 1. Controllo parametri
		if (!Utilities.ValidateInputs.AreValid(req.Neg, req.Ti, req.Tr, req.D, req.T, req.Tot, req.Ck, req.H, out string error))
		{
			return View(new VerifyViewModel
			{
				Stato = "ERRORE",
				StatusText = error,
				BgColor = "#c0392b",
			});
		}

		var emissioneScontrino = new DateTime(
			year: int.Parse(req.D.Substring(4, 4)),
			month: int.Parse(req.D.Substring(2, 2)),
			day: int.Parse(req.D.Substring(0, 2)),
			hour: int.Parse(req.T.Substring(0, 2)),
			minute: int.Parse(req.T.Substring(2, 2)),
			second: int.Parse(req.T.Substring(4, 2)),
			DateTimeKind.Local)
		;
		if (DateTime.Now < emissioneScontrino)
		{
			return View(VerifyViewModel.Falso(new string[] {}));

        }

		try
		{
			string uniqueIdentifier = $"{req.Neg}_{req.Ti}_{req.Tr}_{req.Ck}_{req.H}";
			VerifyViewModel result = await m_VerifyService.VerifyAsync(req, uniqueIdentifier, queryString);
			return View(result);
		}
		catch (Exception ex)
		{
			return View(new VerifyViewModel
			{
				Stato = "ERRORE",
				StatusText = ex.Message,
				BgColor = "#c0392b",
			});
		}
	}

}
