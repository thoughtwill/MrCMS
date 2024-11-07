using System;
using Microsoft.AspNetCore.Mvc.Controllers;
using MrCMS.ACL;
using System.Collections.Generic;
using System.Reflection;

namespace MrCMS.Website.Auth
{
    public class GetAclKeys : IGetACLKeys
    {
        private readonly IAclKeyGenerator _aclKeyGenerator;

        public GetAclKeys(IAclKeyGenerator aclKeyGenerator)
        {
            _aclKeyGenerator = aclKeyGenerator;
        }

        private string GetKey(Type type, string operation)
        {
            return _aclKeyGenerator.GetKey(type.Name, operation);
        }

        public IReadOnlyList<string> GetKeys(Type type, string operation)
        {
            return new List<string>
            {
                GetKey(type, operation)
            };
        }

        public IReadOnlyList<string> GetKeys<TAclRule>(string operation) where TAclRule : ACLRule
        {
            return GetKeys(typeof(TAclRule), operation);
        }
    }}