using Microsoft.AspNetCore.Mvc;
using WEBSITE_TRAVELBOOKING.Core;

namespace WEBSITE_TRAVELBOOKING.Areas.Partner.Controllers
{
    [ValidatePartnerId]
    public class PartnerBaseController : Controller
    {
        protected string PartnerId => HttpContext.Session.GetString("partnerId");

    }
}
