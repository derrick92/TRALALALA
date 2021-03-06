﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using DataAccess.Repos;
using DataAccess.CustomExceptions;
using System.Data.Common;
namespace BusinessLayer
{
    public class UserBL
    {

        public IEnumerable<User> GetAllUsers()
        {
            return new UserRepo().GetAllUsers();
        }

        public User GetUserByID(int id)
        {
            return new UserRepo().GetUserByID(id);
        }

        public User GetUserByUsername(string name)
        {
            return new UserRepo().GetUserByUsername(name);
        }

        public User GetUserByUser(User u)
        {
            return new UserRepo().GetUserByUsername(u.Username);
        }

        public void DeleteUserDropAllRoles(int id)
        {
            List<Role> rlist = new RoleRepo().GetRoles().ToList();

            List<Product> plist = new ProductRepo().GetProductsByCreator(id).ToList();

            foreach (Product zp in plist)
            {
                new ProductRepo().DeleteProduct(zp.ProductID);
            }

            foreach (Role zr in rlist)
            {
                if (new RoleRepo().IsInRole(new UserRepo().GetUserByID(id), zr))
                {
                    UserRepo ur = new UserRepo();
                    RoleRepo rr = new RoleRepo();
                    ur.Entity = rr.Entity;
                    User u = ur.GetUserByID(id);
                    Role r = rr.GetRoleById(zr.RoleID);
                    rr.DeallocateUser(u, r);
                }
            }
            new UserRepo().DeleteUser(id);
        }

        public void Update(User gb)
        {
            new UserRepo().UpdateUser(gb);
        }

        public void CreateUser(User gb)
        {
            UserRepo x = new UserRepo();
            if (!x.DoesUsernameExist(gb.Username) && !x.DoesEmailExist(gb.Email))
            {
                x.CreateUser(gb);
                RoleRepo rr = new RoleRepo();
                x.Entity = rr.Entity;
                x.Entity.Connection.Open();

                DbTransaction transaction = x.Entity.Connection.BeginTransaction();
                try
                {
                    AddDefaultRole(gb.Username);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw new RegistrationException();
                }
                finally
                {
                    x.Entity.Connection.Close();
                }
            }
        }

        public bool isAuthenticated(string username, string password)
        {
            return new UserRepo().isAuthenticated(username, password);
        }

        public void AddDefaultRole(string username)
        {
            UserRepo ur = new UserRepo();
            RoleRepo rr = new RoleRepo();
            ur.Entity = rr.Entity;
            User u = ur.GetUserByUsername(username);
            Role r = rr.GetRoleById(2);
            rr.AllocateRole(u, r);
        }

        public void BecomeSeller(string username)
        {
            UserRepo ur = new UserRepo();
            RoleRepo rr = new RoleRepo();
            ur.Entity = rr.Entity;
            User u = ur.GetUserByUsername(username);
            Role r = rr.GetRoleById(4);
            rr.AllocateRole(u, r);
        }

        public bool DoesUserNameExist(string username)
        {
            return new UserRepo().DoesUsernameExist(username);
        }


        public bool DoesEmailExist(string email)
        {
            return new UserRepo().DoesEmailExist(email);
        }




    }
}
