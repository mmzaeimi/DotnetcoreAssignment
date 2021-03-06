﻿using Hospitad.Application.DTOs;
using Hospitad.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hospitad.Application.Commands.Departments
{
    public class CreateDepartmentCommand : Request<OperationResult>
    {
        public CreateDepartmentCommand(RequestInfo info) : base(info)
        {

        }   
        
        public string Title { get; set; }
        public int OrganizationId { get; set; }
        public int? ParentDepartmentId { get; set; }
    }
}
