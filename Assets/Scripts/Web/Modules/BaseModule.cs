﻿using System;
using System.IO;
using System.Linq;
using Database;
using FluentValidation;
using Nancy;
using Nancy.ModelBinding;

namespace Web.Modules
{
    public abstract class BaseModule<Model, Request, Response> : NancyModule 
        where Model : DatabaseModel 
        where Response : WebResponse
    {
        protected string header;
        protected InlineValidator<Model> addValidator = new InlineValidator<Model>();
        protected InlineValidator<Model> editValidator = new InlineValidator<Model>();


        protected abstract Model ConvertToModel(Request request);
        protected abstract Response ConvertToResponse(Model model);

        public BaseModule()
        {
        }
        
        protected virtual void Init()
        {
            List();
            Status();
            Add();
            Update();
            Remove();
        }

        protected virtual void List()
        {
            Get($"/{header}/", x =>
            {
                try
                {
                    using (var db = DatabaseManager.Open())
                    {
                        int page = this.Request.Query["page"];

                        // 5 is just an arbitrary value to ensure that we don't try and Page a count of 0
                        int count = this.Request.Query["count"] > 0 ? this.Request.Query["count"] : 5;
                        Model[] models = db.Page<Model>(page, count).Items.ToArray();
                        return models.Select(m => ConvertToResponse(m)).ToArray();
                    }
                }
                catch (Exception ex)
                {
                    return new
                    {
                        responseStatus = "error",
                        error = $"Failed to list {typeof(Model).ToString()}: {ex.Message}."
                    };
                }
            });
        }

        protected virtual void Status()
        {
            Get("/" + header + "/{id}", x =>
            {
                try
                {
                    int id = x.id;
                    using (var db = DatabaseManager.Open())
                    {
                        Response response = ConvertToResponse(db.Single<Model>(id));
                        response.Id = id;
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    return new
                    {
                        responseStatus = "error",
                        error = $"Failed to get responseStatus for {typeof(Model).ToString()}: {ex.Message}."
                    };
                }
            });
        }

        protected virtual void Add()
        {
            Post($"/{header}", x =>
            {
                try
                {
                    using (var db = DatabaseManager.Open())
                    {
                        var boundObj = this.Bind<Request>();
                        var model = ConvertToModel(boundObj);
                        addValidator.ValidateAndThrow(model);
                        
                        object id = db.Insert(model);
                        return ConvertToResponse(model);
                    }
                }
                catch (Exception ex)
                { 
                    return new
                    {
                        responseStatus = "error",
                        error = $"Failed to add {typeof(Model).ToString()}: {ex.Message}."
                    };
                }
            });
        }

        protected virtual void Update()
        {
            Put("/" + header + "/{id}", x =>
            {
                try
                {
                    using (var db = DatabaseManager.Open())
                    {
                        var boundObj = this.Bind<Request>();
                        Model model = ConvertToModel(boundObj);
                        model.Id = x.id;
                        editValidator.ValidateAndThrow(model);

                        // NOTE: condition is wrong, we should check for less then zero and more then one independently
                        if (db.Update(model) <= 0)
                        {
                            throw new Exception($"{header} does not exist");
                        }
                        return ConvertToResponse(model);
                    }
                }
                catch (Exception ex)
                {
                    return new
                    {
                        responseStatus = "error",
                        error = $"Failed to update {typeof(Model).ToString()}: {ex.Message}."
                    };
                }
            });
        }

        protected virtual void Remove()
        {
            Delete("/" + header + "/{id}", x =>
            {
                try
                {
                    using (var db = DatabaseManager.Open())
                    {
                        int id = x.id;

                        // NOTE: condition is wrong, we should check for less then zero and more then one independently
                        if (db.Delete<Model>(id) <= 0)
                        {
                            throw new Exception("object does not exist");
                        }
                    }

                    return new { responseStatus = "success" };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        responseStatus = "error",
                        error = $"Failed to remove {typeof(Model).ToString()}: {ex.Message}."
                    };
                }
            });
        }

        // TODO:
        // set responseStatus
        // if url is new:
        // if url starts with file:// and file exists set localPath, if it does not exist throw an exception
        // if url starts with http:// or https:// create a temporary file and initiate downloading
        //    when downloading is completed move file to expected location and update localPath
        // otherwise throw exception with error message
        protected static bool BeValidFilePath(string url)
        {
            Uri uri = new Uri(url);
            if (uri.IsFile)
            {
                if (!File.Exists(uri.LocalPath))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
