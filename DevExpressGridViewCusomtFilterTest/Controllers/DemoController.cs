using System.Web.Mvc;
using System.Web.UI;
using DevExpress.Web.Internal;
using DevExpress.Web.Mvc;
using System.Threading.Tasks;

namespace DevExpress.Web.Demos
{
    public partial class DemoController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.PageSize = 5;
            return View("Index");
        }

        public ActionResult AdvancedCustomBindingPagingAction(GridViewPagerState pager)
        {
            var viewModel = GridViewExtension.GetViewModel("gridView");
            viewModel.ApplyPagingState(pager);
            return AdvancedCustomBindingCore(viewModel);
        }

        public ActionResult AdvancedCustomBindingPartial()
        {
			var viewModel = GridViewExtension.GetViewModel("gridView");
			if (viewModel == null)
				viewModel = CreateGridViewModelWithSummary();
			return AdvancedCustomBindingCore(viewModel);
        }

        public ActionResult AdvancedCustomBindingCore(GridViewModel viewModel)
        {
            try
            {
				viewModel.ProcessCustomBinding(GridViewCustomBindingHandlers.GetDataRowCountAdvanced, GridViewCustomBindingHandlers.GetDataAdvanced);
				return PartialView("_GridView", viewModel);;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public static GridViewModel CreateGridViewModelWithSummary()
        {
            var viewModel = new GridViewModel();
            viewModel.KeyFieldName = "appointmentid";
            viewModel.Columns.Add("appointmentid");
            viewModel.Columns.Add("policyid");
            viewModel.Columns.Add("policynumber");
            return viewModel;
        }
    }
}
