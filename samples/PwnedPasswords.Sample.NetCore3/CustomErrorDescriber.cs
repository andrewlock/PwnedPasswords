using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PwnedPasswords.Validator;

namespace PwnedPasswords.Sample.NetCore3
{
    public class CustomErrorDescriber : PwnedPasswordErrorDescriber
    {
        public override IdentityError PwnedPassword()
        {
            return new IdentityError
            {
                Code = nameof(PwnedPassword),
                Description = "The password you entered has appeared in a data breach. Please choose a different password."
            };
        }
    }
}
