using AuthenticationApi.Helper;
using AuthenticationApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace AuthenticationApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private DbHelper dbHelper;
        public AuthenticationController(IConfiguration configuration)
        {
            dbHelper = new DbHelper(configuration);
        }
///<summary>        
/// Gelen istek doğrultusunda token çözülür.Çözülen değerin zamanı kıyaslanır.Doğru veye yalnış değer döner.
/// Dönen değer sonucu doğru ise işlem başarılı.Yalnış ise elde tutulan veriler gelen verilerin username ve password kontrol edilir.
/// Eşlesme yoksa başarısız varsa zamanı devam ettirmek için yeni token oluşturulur.--AT
/// </summary>
        [HttpPost]
        public LoginResponseModel Login([FromBody] LoginRequestModel reqModel)
        {
            LoginResponseModel responseModel = new LoginResponseModel();
            var isValidToken = IsValid(reqModel.Token);
            if (!isValidToken)
            {
                var verifiedUser = dbHelper.CheckUser(reqModel.Username, reqModel.Password);

                if (!verifiedUser)
                {
                    responseModel.Status = (int)HttpStatusCode.Unauthorized;
                    responseModel.Message = "unsuccessful";
                    return responseModel;
                }

                reqModel.Token = CreateToken(reqModel.Username);
            }
            responseModel.Status = (int)HttpStatusCode.OK;
            responseModel.Message = "success";
            responseModel.Token = reqModel.Token;
            return responseModel;
        }
//Gelen token çözülür.Çözülen token stüne 60dk eklenen zaman büyükse simdiki zamandan işlem true devam eder..--AT
        private bool IsValid(string token)
        {
            JwtSecurityToken jwtSecurityToken;
            try
            {
                jwtSecurityToken = new JwtSecurityToken(token);
            }
            catch (Exception)
            {
                return false;
            }
            return jwtSecurityToken.ValidTo > DateTime.UtcNow;
        }
//Yeni token yaratma;username, userinfo değerine atanır,expires simdiki zamana 60dk üstününe ekleme yapar --AT 
        public string CreateToken(string username)
        {
            var userInfo = new Claim[]{
                new Claim(JwtRegisteredClaimNames.UniqueName,username)
            };

            var token = new JwtSecurityToken(
                claims: userInfo,
                expires: DateTime.Now.AddMinutes(60)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

