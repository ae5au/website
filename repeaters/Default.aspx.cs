﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class repeaters_Default : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (!Page.IsPostBack)
		{
			getPublicRepeaterList();
		}
	}


	protected void btnSearch_Click(object sender, EventArgs e)
	{
		getPublicRepeaterList();
	}

	protected void getPublicRepeaterList()
	{
		string uri = System.Configuration.ConfigurationManager.AppSettings["webServiceRootUrl"] + "ListPublicRepeaters?state=ar";

		Regex rxLatLon = new Regex(@"([-+]?(?:[0-9]|[1-9][0-9])\.\d+),\s*([-+]?(?:[0-9]|[1-9][0-9]|[1-9][0-9][0-9])\.\d+)");

		if (rxLatLon.IsMatch(txtSearch.Text))
		{
			MatchCollection matches = rxLatLon.Matches(txtSearch.Text);
			Match match = matches[0];
			GroupCollection groups = match.Groups;
			uri += string.Format("&latitude={0}&longitude={1}&miles={2}", groups[1], groups[2], "40");
		}
		else
		{
			uri += string.Format("&search={0}", txtSearch.Text);
		}

		using (var webClient = new System.Net.WebClient())
		{
			System.Diagnostics.Debug.WriteLine(uri);
			string json = webClient.DownloadString(uri);
			dynamic data = JsonConvert.DeserializeObject<dynamic>(json);

			string rtn = "";
			rtn += @"
					<table class='repeaterListTable'>
					<thead><tr>
						<th>Frequency</th>
						<th>Offset</th>
						<th>Callsign</th>
						<th>Trustee</th>
						<th>Status</th>
						<th>City</th>
						<th>Attributes</th>
					</tr></thead>";
			rtn += "<tbody>";

			foreach (dynamic obj in data)
			{
				rtn += "<tr>";

				rtn += "<td>" + obj.OutputFrequency + "</td>";
				rtn += "<td>" + obj.Offset + "</td>";
				rtn += "<td>" + obj.Callsign + "</td>";
				rtn += "<td>" + obj.Trustee + "</td>";
				rtn += "<td>" + obj.Status + "</td>";
				rtn += "<td>" + obj.City + "</td>";


				rtn += "<td>";

				List<string> attributes = new List<string>();

				Utilities.GetValueIfNotNull(obj.Analog_InputAccess, "input tone: ", attributes);
				Utilities.GetValueIfNotNull(obj.Analog_OutputAccess, "output tone: ", attributes);
				Utilities.GetValueIfNotNull(obj.DSTAR_Module, "D-Star module: ", attributes);
				Utilities.GetValueIfNotNull(obj.DMR_ID, "DMR ID: ", attributes);
				Utilities.GetNameIfNotNull(obj.AutoPatch, "autopatch", attributes);
				Utilities.GetNameIfNotNull(obj.EmergencyPower, "emergency power", attributes);
				Utilities.GetNameIfNotNull(obj.Linked, "linked", attributes);
				Utilities.GetNameIfNotNull(obj.RACES, "RACES", attributes);
				Utilities.GetNameIfNotNull(obj.ARES, "ARES", attributes);
				Utilities.GetNameIfNotNull(obj.Weather, "weather net", attributes);

				for (int index = 0; index < attributes.Count; index++)
				{
					string attribute = attributes[index];
					rtn += index < attributes.Count - 1 ? attribute + ", " : attribute;
				}

				rtn += "</td></tr>";
			}

			rtn += "</tbody></table>";

			repeaterList.Text = rtn;
		}
	}
}