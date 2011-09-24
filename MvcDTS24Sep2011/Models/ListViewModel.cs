using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MvcDTS.Models
{
	
    public class DriveInOutViewModel
    {
		public int No { get; set; }
		public string SubInv { get; set; }
		public string FamilyName { get; set; }
		public string DESCRIPT {get; set;}
		public List<int> CountBackDays { get; set; }
		//public List<int> allTotals { get; set; }
		//public int CountBackDays0_30 { get; set; }
		//public int CountBackDays31_60 { get; set; }
		//public int CountBackDays61_90 { get; set; }
		//public int CountBackDays91_120 { get; set; }
		//public int CountBackDays121 { get; set; }
    }

	public class DriveInfo
	{
		public string subInv { get; set; }
		public string serialNo { get; set; }
		public DateTime eventDate { get; set; }
		//public int totals { get; set; }
		public string FamilyName { get; set; }
		public bool hasFamilyName { get; set; }
	}

	
	
}