using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace QuanLyVeTau.Models
{
    public class CustomRoleProvider : RoleProvider
    {
        private readonly QuanLyVeTauDBDataContext db;


        public CustomRoleProvider()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["QL_VETAUConnectionString"].ConnectionString;
            db = new QuanLyVeTauDBDataContext(connectionString);
        }
        public override string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            var user = db.TaiKhoans.FirstOrDefault(t => t.Email == username && t.DaXoa == false);
            if (user != null)
            {
                return new string[] { "User" };
            }

            var employee = db.TaiKhoanNhanViens.FirstOrDefault(t => t.Email == username && t.DaXoa == false);
            if (employee != null)
            {
                return new string[] { employee.VaiTro };
            }

            throw new NotImplementedException("User not found");
        }


        public override string[] GetUsersInRole(string roleName)
        {
            return db.TaiKhoanNhanViens
                .Where(tk => !tk.DaXoa.GetValueOrDefault())
                .Select(tk => tk.VaiTro)
                .ToArray();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            
            TaiKhoan tk = db.TaiKhoans.FirstOrDefault(t => t.Email == username);

            if (tk != null)
            {
                return true;
            }

            var employee = db.TaiKhoanNhanViens
                             .FirstOrDefault(tkNhanVien => tkNhanVien.Email == username
                                                           && tkNhanVien.VaiTro == roleName
                                                           && !(tkNhanVien.DaXoa ?? false));

            return employee != null;
        }



        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}