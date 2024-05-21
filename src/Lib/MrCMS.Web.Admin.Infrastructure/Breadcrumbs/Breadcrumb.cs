using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MrCMS.Entities;
using MrCMS.Helpers;
using System.Collections.Generic;
using System.Linq;
using MrCMS.Web.Admin.Infrastructure.Models;

namespace MrCMS.Web.Admin.Infrastructure.Breadcrumbs
{
    // ReSharper disable once UnusedTypeParameter -- used to establish hierarchy when read via reflection
    public abstract class Breadcrumb<T> : Breadcrumb where T : Breadcrumb
    {
    }

    public abstract class Breadcrumb
    {
        public virtual string Title => Name;
        // default to type name with Breadcrumb removed

        protected Breadcrumb()
        {
            Name = GetType().Name.Replace("Breadcrumb", "").BreakUpString();
        }

        public virtual string Name { get; protected set; }

        public virtual decimal Order => 0;

        public virtual bool IsNav => false;

        public abstract string Controller { get; }
        public abstract string Action { get; }
        public virtual int? Id => GetIdFromArguments(ActionArguments);
        public virtual IDictionary<string, object> ActionArguments { get; set; }

        public virtual string Url(IUrlHelper url)
        {
            if (IsPlaceHolder)
            {
                return null;
            }

            var routeValues = new RouteValueDictionary(new { Id, area = "Admin" });

            if (ActionArguments != null)
            {
                foreach (var arg in ActionArguments)
                {
                    if (arg.Key != "id" && arg.Value is not (SystemEntity or IHaveId))
                    {
                        routeValues[arg.Key] = arg.Value;
                    }
                }
            }

            return url.Action(Action, Controller, routeValues);
        }

        public virtual bool OpenInNewWindow { get; }

        /// <summary>
        /// If this is true, it will use the parent id to get another of the same type rather than go to the parent type 
        /// </summary>
        public virtual bool Hierarchical { get; }

        public virtual string CssClass { get; }

        /// <summary>
        /// Used to bypass check for descriptor/ACL as there isn't a corresponding route
        /// </summary>
        public virtual bool IsPlaceHolder { get; }


        public int? ParentId => GetIdFromArguments(ParentActionArguments);
        public virtual IDictionary<string, object> ParentActionArguments { get; set; }
        public virtual bool ShouldSkip { get; }

        /// <summary>
        /// This is to be called once to initialize the breadcrumb with data.
        /// Should be used for getting the data for parent ids and names
        /// </summary>
        public virtual void Populate()
        {
        }

        private int? GetIdFromArguments(IDictionary<string, object> arguments)
        {
            if (arguments == null)
            {
                return null;
            }

            int? id = null;
            if (arguments.ContainsKey("id") &&
                int.TryParse(arguments["id"]?.ToString(), out int idVal))
            {
                id = idVal;
            }
            else if (arguments.Values.OfType<SystemEntity>().Any())
            {
                id = arguments.Values.OfType<SystemEntity>().First().Id;
            }
            else if (arguments.Values.OfType<IHaveId>().Any())
            {
                id = arguments.Values.OfType<IHaveId>().First().Id;
            }

            return id;
        }

        protected static IDictionary<string, object> CreateIdArguments(int? id, object additionalData = null)
        {
            var arguments = new RouteValueDictionary {["id"] = id};
            if (additionalData != null)
            {
                var properties = additionalData.GetType().GetProperties();
                foreach (var property in properties)
                {
                    arguments[property.Name] = property.GetValue(additionalData);
                }
            }

            return arguments;
        }
    }
}