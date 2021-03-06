﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CoyoteMoves.Models.RequestItems;
using CoyoteMoves.Data_Access;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.IO;
using CoyoteMoves.Models.SeatingData;
using CoyoteMoves.Models.EmployeeData;
using CoyoteMoves.Emailer.Models;
using CoyoteMoves.Emailer;
using System.Collections.ObjectModel;

namespace CoyoteMoves.Controllers
{
    public class RequestFormController : ApiController
    {
        // POST api/RequestForm/SendChangeRequest
        public HttpResponseMessage SendChangeRequest(JObject json)
        {
            int managerID = GetIDFromName((string)json["current"]["bazookaInfo"]["managerId"]);
            int f_managerID = GetIDFromName((string)json["future"]["bazookaInfo"]["managerId"]);

            HttpResponseMessage useForFailure = new HttpResponseMessage(HttpStatusCode.NotFound);
            if ((managerID == -1) && (f_managerID == -1))
            {
                useForFailure.Content = new StringContent("Both current and future managers were not found.");
                return useForFailure;
            }
            else if (managerID == -1)
            {
                useForFailure.Content = new StringContent("Current manager was not found.");
                return useForFailure;
            }
            else if (f_managerID == -1)
            {
                useForFailure.Content = new StringContent("Future manager was not found.");
                return useForFailure;
            }

            RequestForm obj = makeRequestForm(json);

            Collection<string> to = new Collection<string>();
            to.Add("jake.marsh@coyote.com");
            EmailSender emailer = new EmailSender("Coyote Moves Request ", to, "coyotemoves@coyote.com", "", HttpContext.Current.Server.MapPath("/CoyoteMoves/CoyoteMovesTemplate.pdf"));
            emailer.sendMovesRequest(obj);

            RequestFormDB formDB = new RequestFormDB();
            formDB.StoreRequestFormInDatabaseAsPending(obj);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        public RequestForm makeRequestForm(JObject json)
        {
            string currManagerName = (string)json["current"]["bazookaInfo"]["managerId"];
            string futureManagerName = (string)json["future"]["bazookaInfo"]["managerId"];
            int managerID = GetIDFromName(currManagerName);
            int f_managerID = GetIDFromName(futureManagerName);
            json["current"]["bazookaInfo"]["managerId"] = managerID;
            json["current"]["ultiproInfo"]["supervisor"] = managerID;
            json["future"]["bazookaInfo"]["managerId"] = f_managerID;
            json["future"]["ultiproInfo"]["supervisor"] = f_managerID;

            UserController uc = new UserController();
            string[] name = uc.GetUserName().Split('.');
            string creatorName = name[0] + " " + name[1];
            int creatorID = GetIDFromName(creatorName);

            RequestForm obj = null;
            using (var sr = new StringReader(json.ToString()))
            using (var jr = new JsonTextReader(sr))
            {
                var js = new JsonSerializer();
                obj = (RequestForm)js.Deserialize<RequestForm>(jr);
            }
            obj.EmployeeId = GetIDFromName((string)json["name"]);
            obj.CreatedByID = creatorID;
            obj.Current.BazookaInfo.SecurityItemRights = "";
            obj.ReviewInfo.FilesToBeRemovedFrom = "(" + obj.Current.BazookaInfo.Group + ")" + currManagerName.Replace(" ", ".")+" "+obj.ReviewInfo.FilesToBeRemovedFrom;
            obj.ReviewInfo.FilesToBeAddedTo = "(" + obj.Future.BazookaInfo.Group + ")" + futureManagerName.Replace(" ", ".")+" "+obj.ReviewInfo.FilesToBeAddedTo;

            return obj;

        }

        //GET api/RequestForm/GetIDFromName
        public int GetIDFromName(string name)
        {
            EmployeeDB edb = new EmployeeDB();
            return edb.GetIdFromName(name);
        }

        // GET api/RequestForm/GetAllGroups
        public List<string> GetAllGroups()
        {
            RequestDataDB dbaccess = new RequestDataDB();
            return dbaccess.GetAllGroups();
        }

        // GET api/RequestForm/GetAllDepartments
        public List<string> GetAllDepartments()
        {
            RequestDataDB dbaccess = new RequestDataDB();
            return dbaccess.GetAllDepartments();
        }

        // GET api/RequestForm/GetAllJobTitles
        public List<string> GetAllJobTitles()
        {
            RequestDataDB dbaccess = new RequestDataDB();
            return dbaccess.GetAllJobTitles();
        }

        /*
        * INB4: This should probably go in another class, just writing it down so I don't forget
        * Once the changes have been requested and have been approved at HR and service desk,
        * we'll then have to update the database(s)
        * */
        public void UpdateDataSourcesWithApprovedChanges()
        {
            //idk what it should take in, or what it should return...

            //remove this change request from the "unapproved" queue and (move it to the "approved" queue? or just delete it?)

            //send the updated info to the proper data source (have to talk to Bazooka, Active Directory, Cisco, (Ultipro?)
            //probably a helper function for updating each source...
        }

    }
}
