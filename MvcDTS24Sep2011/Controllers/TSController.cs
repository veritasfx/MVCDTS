using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcDTS.Models;

namespace MvcDTS.Controllers
{
    public class TSController : Controller
    {
        //
        // GET: /TS/

		private TSEntities _entities = new TSEntities();
		private int _pageSize = 100; //15;//

		public ActionResult Index(string sort, int? page, string curSubInvCode, string curFamilyName, bool? showFamilyName)
		{
			var currentPage = page ?? 1;
			ViewData["SortItem"] = sort;
			sort = sort ?? "No";
			ViewData["PageSize"] = _pageSize;
			bool curShowFamilyName = showFamilyName ?? false;

			List<DriveInOutViewModel> viewModel = new List<DriveInOutViewModel>();

			if (curFamilyName == null)
			{
				curFamilyName = "All";
			}
			ViewData["curFamilyName"] = curFamilyName;

			string[] ValidSICs = { "TLS", "FW", "ECP", "EJQ", "SGN", "FQT", "SEC" };
			List<WIP_CODE_MAP_org> SICs = _entities.WIP_CODE_MAP_org.Where(a => a.AREA_CODE == "default" && ValidSICs.Contains(a.SUB_INV_CODE)).OrderBy(a => a.SUB_INV_CODE).ToList();

			ViewData["SUB_INV_CODEs"] = SICs.Select(a => a.SUB_INV_CODE).ToList();

			if (curSubInvCode == null)
			{
				curSubInvCode = "All";
			}
			ViewData["curSubInvCode"] = curSubInvCode;


			var myWIP_EVENT = (from r in _entities.WIP_STATE
								   join h in _entities.WIP_EVENT on r.SERIAL_NUM equals h.SERIAL_NUM
								   where (ValidSICs.Contains(r.SUB_INV_CODE) && curSubInvCode == "All" || r.SUB_INV_CODE == curSubInvCode) 
								   orderby r.SERIAL_NUM
								   select new
								   {
									   r.SERIAL_NUM,
									   r.SUB_INV_CODE,
									   FamilyName = r.PART_NUM.Substring(0, 3), // b.FAMILY,  
									   h.EVENT_DATE
								   } into t1
								   group t1 by t1.SERIAL_NUM into tempDB
								   select new DriveInfo
								   {
									   eventDate = tempDB.OrderBy(a => a.EVENT_DATE).Select(a => a.EVENT_DATE).FirstOrDefault(),
									   serialNo = tempDB.FirstOrDefault().SERIAL_NUM,
									   subInv = tempDB.FirstOrDefault().SUB_INV_CODE,
									   FamilyName = tempDB.FirstOrDefault().FamilyName,
									   hasFamilyName = false,
								   }).OrderBy(a=>a.subInv).ToList();

			List<PART_NUM_MAP> partNumFamilyMaps = (from r in _entities.PART_NUM_MAP
													where r.PART_NUM.Substring(3, 7) == "000-000"
													select r).Distinct().ToList();
			
			foreach (PART_NUM_MAP pnfm in partNumFamilyMaps)
			{
					if (myWIP_EVENT.Any(a => a.FamilyName == pnfm.FAMILY))
					{
						List<DriveInfo> driveInfos = myWIP_EVENT.Where(a => a.FamilyName == pnfm.FAMILY).ToList();
						foreach (DriveInfo driveInfo in driveInfos)
						{
							driveInfo.FamilyName = pnfm.DESCRIP;
							driveInfo.hasFamilyName = true;
						}
					}
			}

			foreach (DriveInfo driveInfo in myWIP_EVENT)
			{
					if (!driveInfo.hasFamilyName)
					{
						driveInfo.FamilyName = "NO DESCRIPTION";
					}
			}

				myWIP_EVENT = (from r in myWIP_EVENT where (r.FamilyName == curFamilyName || curFamilyName == "All") && (r.subInv == curSubInvCode || curSubInvCode == "All")  orderby r.subInv, r.FamilyName select r).ToList();

				ViewData["FamilyNames"] = myWIP_EVENT.Select(a=>a.FamilyName).Distinct().ToList();

				DateTime dayInit = new DateTime(2010, 1, 1);
				DateTime[] sepDateList = new DateTime[14] { 
                DateTime.Today, 
                DateTime.Today.AddDays(-30), 
                DateTime.Today.AddDays(-60), 
                DateTime.Today.AddDays(-90), 
                DateTime.Today.AddDays(-120), 
                DateTime.Today.AddDays(-150), 
                DateTime.Today.AddDays(-180), 
                DateTime.Today.AddDays(-210), 
                DateTime.Today.AddDays(-240), 
                DateTime.Today.AddDays(-270), 
                DateTime.Today.AddDays(-300), 
                DateTime.Today.AddDays(-330), 
                DateTime.Today.AddDays(-360), 
                dayInit };

				dayInit = myWIP_EVENT.OrderBy(a => a.eventDate).Select(a => a.eventDate).FirstOrDefault();
			int subInvSUM = myWIP_EVENT.Select(a=>a.subInv).Distinct().Count();

				int[,] totals = new int[subInvSUM+1, sepDateList.Count()];
				int subInvIndex = 0;
				int count = 0; 
				var familyNames = myWIP_EVENT.Select(a => new { a.subInv, a.FamilyName}).Distinct().OrderBy(a => a.subInv).ThenBy(a => a.FamilyName).ToList();
				foreach (var di in familyNames)
				{
					DriveInOutViewModel diovm = new DriveInOutViewModel();
					string subInvDes = SICs.Single(a => a.SUB_INV_CODE == di.subInv).DESCRIP;
					diovm.SubInv = di.subInv;
					diovm.FamilyName = di.FamilyName;
					diovm.CountBackDays = new List<int>();
					diovm.No = count + 1;

					int subInvFamilyTotals = 0;
					for (int i = 0; i < sepDateList.Count() - 1; i++)
					{
						int tTotals = CalculateCount(myWIP_EVENT, dayInit, di.subInv, di.FamilyName, sepDateList[i + 1], sepDateList[i]);
						diovm.CountBackDays.Add(tTotals);
						subInvFamilyTotals += tTotals;
						totals[subInvIndex, i] += tTotals;
					}
					diovm.CountBackDays.Add(subInvFamilyTotals);
					if (curShowFamilyName)
					{
						viewModel.Add(diovm);
					}

					if (count < familyNames.Count-1 &&  di.subInv != familyNames[count + 1].subInv || count == familyNames.Count -1 )
					{
						DriveInOutViewModel diovmEnd = new DriveInOutViewModel();
						diovmEnd.SubInv = di.subInv;
						if (curShowFamilyName)
						{
							diovmEnd.FamilyName = " (" + subInvDes + ")";
						}
						else
						{
							diovmEnd.FamilyName = subInvDes;
						}
						if (curShowFamilyName)
						{
							diovmEnd.No = 0;
						}
						else
						{
							diovmEnd.No = subInvIndex + 1;
						}
						diovmEnd.CountBackDays = new List<int>();
						for (int k = 0; k < sepDateList.Count() -1; k++)
						{
							diovmEnd.CountBackDays.Add(totals[subInvIndex, k]);
							totals[subInvIndex, sepDateList.Count()-1] += totals[subInvIndex, k];
							totals[subInvSUM, k] += totals[subInvIndex, k];
						}
						diovmEnd.CountBackDays.Add(totals[subInvIndex, sepDateList.Count()-1]);
						viewModel.Add(diovmEnd);
						totals[subInvSUM, sepDateList.Count()-1] += totals[subInvIndex, sepDateList.Count()-1];
						subInvIndex++;
					}
					count++;
				}

				if (curSubInvCode == "All")
				{
					DriveInOutViewModel diovmSum = new DriveInOutViewModel();
					diovmSum.SubInv = "SUM()";
					//diovmSum.No = 0;
					diovmSum.CountBackDays = new List<int>();
					for (int i = 0; i < sepDateList.Count() ; i++)
					{
						diovmSum.CountBackDays.Add(totals[subInvSUM, i]);
					}
					viewModel.Add(diovmSum);
				}

			TempData["viewModel"] = viewModel;
			TempData["headers"] = GetHeaderList(sepDateList, curShowFamilyName);
			ViewData["Headers"] = GetHeaderList(sepDateList, curShowFamilyName);
			ViewData["ShowFamilyName"] = curShowFamilyName;

			ViewData["TotalPages"] = (int)Math.Ceiling((float)viewModel.Count() / _pageSize);

			if ((int)ViewData["TotalPages"] < currentPage)
			{
				currentPage = 1;
			}
			ViewData["CurrentPage"] = currentPage;

			viewModel = (viewModel.AsQueryable().Skip((currentPage - 1) * _pageSize).Take(_pageSize)).ToList();

			return View(viewModel);
		}

		//public ActionResult Index(string sort, int? page, int? curSubInvCodeID, int? curFamilyNameID)
		//{
		//    var currentPage = page ?? 1;
		//    ViewData["SortItem"] = sort;
		//    sort = sort ?? "No";
		//    ViewData["PageSize"] = _pageSize;

		//    List<DriveInOutViewModel> viewModel = new List<DriveInOutViewModel>();

		//    DateTime dayInit = new DateTime(2010, 1, 1);
		//    DateTime[] sepDateList = new DateTime[14] { 
		//        DateTime.Today, 
		//        DateTime.Today.AddDays(-30), 
		//        DateTime.Today.AddDays(-60), 
		//        DateTime.Today.AddDays(-90), 
		//        DateTime.Today.AddDays(-120), 
		//        DateTime.Today.AddDays(-150), 
		//        DateTime.Today.AddDays(-180), 
		//        DateTime.Today.AddDays(-210), 
		//        DateTime.Today.AddDays(-240), 
		//        DateTime.Today.AddDays(-270), 
		//        DateTime.Today.AddDays(-300), 
		//        DateTime.Today.AddDays(-330), 
		//        DateTime.Today.AddDays(-360), 
		//        dayInit };

		//    int[] totals = new int[sepDateList.Count()];

		//    List<PART_NUM_MAP> partNumFamilyMaps = (from r in _entities.PART_NUM_MAP
		//                                            where r.PART_NUM.Substring(3, 7) == "000-000"
		//                                            select r).Distinct().ToList();
		//    ViewData["FamilyNames"] = partNumFamilyMaps.OrderBy(a => a.DESCRIP).Select(a => a.DESCRIP).Distinct().ToList();

		//    if (curFamilyNameID == null)
		//    {
		//        curFamilyNameID = partNumFamilyMaps.Count;
		//    }
		//    ViewData["curFamilyNameID"] = curFamilyNameID;

		//    string[] ValidSICs = { "TLS", "FW",  "ECP",  "EJQ", "SGN", "FQT", "SEC"};
		//    List<WIP_CODE_MAP_org> SICs = _entities.WIP_CODE_MAP_org.Where(a => a.AREA_CODE == "default" && ValidSICs.Contains(a.SUB_INV_CODE)).OrderBy(a => a.SUB_INV_CODE).ToList();

		//    ViewData["SUB_INV_CODEs"] = SICs.Select(a => a.SUB_INV_CODE).ToList();

		//    if (curSubInvCodeID == null)
		//    {
		//        curSubInvCodeID = SICs.Count;
		//    }
		//    ViewData["curSubInvCodeID"] = curSubInvCodeID;

		//    foreach (WIP_CODE_MAP_org wcm in SICs)
		//    {
		//        wcm.SUB_INV_CODE;
                 
		//            //List<WIP_STATE> myWIP_STATE = _entities.WIP_STATE.Where(a => a.SUB_INV_CODE == subInvCode).OrderBy(a=>a.SERIAL_NUM).ToList();

		//            //var tt = (from r in myWIP_STATE
		//            //           join h in _entities.WIP_EVENT on r.SERIAL_NUM equals h.SERIAL_NUM
		//            //           select new DriveInfo
		//            //           {
		//            //                eventDate = h.EVENT_DATE,
		//            //                serialNo = r.SERIAL_NUM,
		//            //                subInv = subInvCode,
		//            //           }).ToList();
							  
		//            //var myWIP_EVENT = (from p in tt group p by p.serialNo into ttt select ttt.OrderByDescending(a => a.eventDate).First()).ToList();

				
		//            var myWIP_EVENT = ( from r in _entities.WIP_STATE
		//                                               //      join l in (from r in _entities.PART_NUM_MAP
		//                                               //      where r.PART_NUM.Substring(3, 7) == "000-000" select r).Distinct().ToList() 
		//                                               //     on r.PART_NUM.Substring(0, 3) equals l.FAMILY into tt 
		//                                               //from b in tt.DefaultIfEmpty()
		//                               join h in _entities.WIP_EVENT on r.SERIAL_NUM equals h.SERIAL_NUM
		//                                where r.SUB_INV_CODE == wcm.SUB_INV_CODE 
		//                               orderby r.SERIAL_NUM//, b.FAMILY
		//                               select new { 
		//                                            r.SERIAL_NUM, 
		//                                            r.SUB_INV_CODE,
		//                                            FamilyName = r.PART_NUM.Substring(0, 3), // b.FAMILY,  
		//                                            //FamilyName = (b != null) ? b.FAMILY : ("NO DESCRIPTION"),
													 
		//                                   h.EVENT_DATE } into t1
									
		//                               group t1 by t1.SERIAL_NUM into tempDB
		//                               select new DriveInfo
		//                               {
		//                                   eventDate = tempDB.OrderBy(a => a.EVENT_DATE).Select(a => a.EVENT_DATE).FirstOrDefault(),
		//                                   serialNo = tempDB.FirstOrDefault().SERIAL_NUM,
		//                                   subInv = wcm.SUB_INV_CODE,
		//                                   FamilyName = tempDB.FirstOrDefault().FamilyName,
		//                                   hasFamilyName = false,
		//                               }).ToList();

		//            foreach (PART_NUM_MAP pnfm in partNumFamilyMaps)
		//            {
		//                if (myWIP_EVENT.Any(a => a.FamilyName == pnfm.FAMILY))
		//                {
		//                    List<DriveInfo> driveInfos = myWIP_EVENT.Where(a => a.FamilyName == pnfm.FAMILY).ToList();
		//                    foreach (DriveInfo driveInfo in driveInfos)
		//                    {
		//                        driveInfo.FamilyName = pnfm.DESCRIP;
		//                        driveInfo.hasFamilyName = true;
		//                    }
		//                }
		//            }

		//            foreach (DriveInfo driveInfo in myWIP_EVENT)
		//            {
		//                if (!driveInfo.hasFamilyName)
		//                {
		//                    driveInfo.FamilyName = "NO DESCRIPTION"; 
		//                }
		//            }
					
		//            dayInit = myWIP_EVENT.OrderBy(a => a.eventDate).Select(a => a.eventDate).FirstOrDefault();
				
		//            int subInvCodeTotals = 0;
		//            List<string> familyNames = myWIP_EVENT.Select(a => a.FamilyName).Distinct().ToList();
		//            foreach (string fn in familyNames)
		//            {
		//                DriveInOutViewModel diovm = new DriveInOutViewModel();
		//                diovm.SubInv = wcm.SUB_INV_CODE + "\r\n (" + wcm.DESCRIP + ")";
		//                //diovm.DESCRIPT = wcm.DESCRIP;
		//                diovm.FamilyName = fn;
		//                diovm.CountBackDays = new List<int>();

		//                int subInvFamilyTotals = 0;
		//                for (int i = 0; i < sepDateList.Count() - 1; i++)
		//                {
		//                    int tTotals = CalculateCount(myWIP_EVENT, dayInit, wcm.SUB_INV_CODE, fn, sepDateList[i + 1], sepDateList[i]);
		//                    diovm.CountBackDays.Add(tTotals);
		//                    subInvFamilyTotals += tTotals;
		//                    //subInvCodeTotals += tTotals;
		//                    totals[i] += tTotals;
		//                }
		//                diovm.CountBackDays.Add(subInvFamilyTotals);
		//                subInvCodeTotals += subInvFamilyTotals;
		//                diovm.CountBackDays.Add(subInvCodeTotals);
		//                viewModel.Add(diovm);
		//            }

				
		//    }

		//    DriveInOutViewModel diovm1 = new DriveInOutViewModel();
		//    diovm1.SubInv = "(SUM)";
		//    diovm1.CountBackDays = new List<int>();
		//    int allTotals = 0;
		//    for (int i = 0; i < sepDateList.Count() - 1; i++)
		//    {
		//        diovm1.CountBackDays.Add(totals[i]);
		//        allTotals += totals[i];
		//    }
		//    diovm1.CountBackDays.Add(allTotals);
		//    diovm1.CountBackDays.Add(allTotals);
		//    viewModel.Add(diovm1);

		//    TempData["viewModel"] = viewModel;
		//    TempData["headers"] = GetHeaderList(sepDateList);
		//    ViewData["Header"] = GetHeaderList(sepDateList);

		//    ViewData["TotalPages"] = (int)Math.Ceiling((float)viewModel.Count() / _pageSize);

		//    if ((int)ViewData["TotalPages"] < currentPage)
		//    {
		//        currentPage = 1;
		//    }
		//    ViewData["CurrentPage"] = currentPage;

		//    return View(viewModel);
		//}

		private static List<string> GetHeaderList(DateTime[] sepDateList, bool showFamilyName)
		{
			List<string> headers = new List<string>();
			headers.Add("No");
            headers.Add("SubInv ");
			//headers.Add("DESCRIPTION");
			if (showFamilyName)
			{
				headers.Add("FamilyName");
			}
			else
			{
				headers.Add("SubInv Description");
			}
			int dayStart = 0;
			for (int i = 0; i < sepDateList.Count()-1; i++)
			{
				int dayEnd = -30 * (i + 1);
				if (i == sepDateList.Count() - 2)
				{
					headers.Add(" > " + (-dayStart).ToString());
				}
				else
				{
					headers.Add((-dayStart).ToString() + " ~ " + (-dayEnd).ToString());
				}
				dayStart = dayEnd - 1;
			}
			headers.Add("TOTAL");
			return headers;
		}

		private int CalculateCount(List<DriveInfo> myWIP_EVENT, DateTime dayInit, string subInvCode, string familyName, DateTime dayStart, DateTime dayEnd)
		{
			int driveIns = (from r in myWIP_EVENT
							where r.eventDate >= dayStart && r.eventDate <= dayEnd && r.subInv == subInvCode && r.FamilyName == familyName
							select r).Count();
			return driveIns;
		}


		/// <summary>
		/// Generates Excel document using supplied headers
		/// </summary>
		public ActionResult GenerateExcel2()
		{
			var viewModel = (List<DriveInOutViewModel>)TempData["viewModel"];
			string[] headers = ((List<string>)TempData["headers"]).ToArray();
			return this.Excel(null, viewModel.AsQueryable(), "data.xls", headers);
		}

		public ActionResult ToExcel()//List<DriveInOutViewModel> report)
		{
			Response.AddHeader("Content-Disposition", "attachment; filename=report.xls");
			Response.ContentType = "application/vnd.ms-excel";
			return View("Index");

			//return ExcelResult.View("Index", report, "report.xls");
		}

    }
}
