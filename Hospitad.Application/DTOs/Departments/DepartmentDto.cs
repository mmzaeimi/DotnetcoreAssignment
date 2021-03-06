﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Hospitad.Application.DTOs.Departments
{
    public class DepartmentDto
    {
        public DepartmentDto()
        {
            SubDepartments = new HashSet<DepartmentDto>();
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public int? ParentDepartmentId { get; set; }
        public int OrganizationId { get; set; }
        public ICollection<DepartmentDto> SubDepartments { get; set; }
    }
}
