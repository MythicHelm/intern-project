﻿using CoyoteMoves.Models.EmployeeData;
using CoyoteMoves.Models.RequestItems;
using CoyoteMoves.Models.SeatingData;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoyoteMoves.Models
{
    public class SqlToDeskModelFactory
    {

        SqlDataReader reader { get; set; }

        public SqlToDeskModelFactory(SqlDataReader reader)
        {
            this.reader = reader;
        }

        public List<Desk> GetAllDesks()
        {
            if (this.reader == null)
            {
                return null;
            }

            List<Desk> DeskList = new List<Desk>();
            while (reader.Read())
            {
                DeskList.Add(CreateDesk());
            }

            return DeskList;
        }

        public List<Desk> GetAllDesks(int floor)
        {
            if (this.reader == null)
            {
                return null;
            }

            List<Desk> DeskList = new List<Desk>();
            while (reader.Read())
            {
                DeskList.Add(CreateDesk());
            }

            return DeskList;
        }

        public Employee CreateEmployee()
        {
            string FirstName = reader["FirstName"].ToString();
            if (FirstName.Equals(""))
            {
                return null;
            }
            else
            {
                string LastName = reader["LastName"].ToString();
                string Email = reader["WorkEmail"].ToString();
                string Phone = reader["WorkPhoneNumber"].ToString();
                string JobTitle = reader["JobTitle"].ToString();
                //template
                string Department = reader["Department"].ToString();
                string Group = reader["Group"].ToString();
                int ID = (int)reader["PersonID"];
                string ManagerName = reader["ManagerFirstName"].ToString() + " " + reader["ManagerLastName"].ToString();
                string Template = reader["Template"].ToString();
                //security items rights
                Employee TempGuy = new Employee()
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    JobTitle = JobTitle,
                    Department = Department,
                    Group = Group,
                    Id = ID,
                    ManagerName = ManagerName,
                    PhoneNumber = Phone,
                    Template = Template
                };

                return TempGuy;
            }
        }

        public Desk CreateDesk()
        {
            string deskNumber = reader["DeskNumber"].ToString();
            CoordinatePoint TopLeft = new CoordinatePoint((double)reader["TopLeftX"], (double)reader["TopLeftY"]);
            double Orientation = (double)reader["Orientation"];
            Location loc = new Location((int)reader["FloorNumber"], TopLeft, Orientation);
            Employee TempGuy = this.CreateEmployee();

            Desk TempDesk = new Desk(loc, deskNumber, TempGuy);
            return TempDesk;
        }
    }
}