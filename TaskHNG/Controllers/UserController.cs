using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TaskHNG.Models;

namespace TaskHNG.Controllers
{

    public class UserController : Controller
    {
        TaskHNG_dbEntities db = new TaskHNG_dbEntities();
        // Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        //Registration POST Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,Activationcode")] UserReg user)
        {
            bool Status = false;
            string message = "";

            //Model Validation
            if (ModelState.IsValid)
            {
                // Email Already Exist
                var isExist = IsEmailExist(user.EmailId);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "This Email Exist");
                    return View(user);
                }                

                //Generate Activation Code                
                user.Activationcode = Guid.NewGuid();

                //Password Hashing
                using (TaskHNG_dbEntities db = new TaskHNG_dbEntities())
                {
                    user.Password = Crypto.Hash(user.Password);
                    user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);
                }
                user.IsEmailVerified = false;

                //Save to Database
                db.UserRegs.Add(user);
                db.SaveChanges();

                //Send verification Email
                SendVerificationLinkEmail(user.EmailId, user.Activationcode.ToString());
                message = "Registration is Successfully Complete. Account Activation Link " + "Has been Sent to your Email:" + user.EmailId;
                Status = true;
            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            
            return View(user);
        }

       //Verify Email Account
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            db.Configuration.ValidateOnSaveEnabled = false; //To avoid issues in db concerning confirm password

            var v = db.UserRegs.Where(x => x.Activationcode == new Guid(id)).FirstOrDefault();
            if (v != null)
            {
                v.IsEmailVerified = true;
                db.SaveChanges();
                Status = true;
            }
            else
            {
                ViewBag.Message = "Invalid Request";
            }

            ViewBag.Status = Status;
            return View();
        }

        //Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        //Login POST
        [HttpPost]
        public ActionResult Login(UserLogin login, string returnUrl="")
        {
            string message = "";

            var v = db.UserRegs.Where(x => x.EmailId == login.EmailId).FirstOrDefault();
            if (v != null)
            {
                if (string.Compare(Crypto.Hash(login.Password), v.Password) == 0)
                {
                    int timeout = login.RememberMe ? 525600 : 25;
                    var ticket = new FormsAuthenticationTicket(login.EmailId, login.RememberMe, timeout);
                    string encrypted = FormsAuthentication.Encrypt(ticket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                    cookie.Expires = DateTime.Now.AddMinutes(timeout);
                    cookie.HttpOnly = true;
                    Response.Cookies.Add(cookie);

                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    message = "Invalid Details";
                }
            }
            else
            {
                message = "Invalid Details provided";
            }
            ViewBag.Message = message;

            return View();
        }

        //Logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }


        [NonAction]
        public Boolean IsEmailExist(string emailId)
        {           
                var v = db.UserRegs.Where(x => x.EmailId == emailId).FirstOrDefault();
                return v != null;
                     
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode)
        {
            var verifyUrl = "/User/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("teamelitehng@gmail.com", "HNG Team Elite");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "123456hng";
            string subject = "Your Account Has Being Successfully Created";
            string body= "<br><br>We are Exicited to Tell You That Your HNG Team Elite Account is" + " successfully. Please Click The Link Below To Verify Your Account " + "<br/><br/><a href= '"+link+"'>"+link+"</a>";

            SmtpClient smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)

            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }
    }
}