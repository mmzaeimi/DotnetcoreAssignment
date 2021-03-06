﻿using Hospitad.Application.DTOs;
using Hospitad.Application.DTOs.Departments;
using Hospitad.Application.Interfaces;
using Hospitad.Application.Queries.Departments;
using Hospitad.Domain.Organizations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hospitad.Application.Handlers.Departments
{
    public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, OperationResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetDepartmentsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OperationResult> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
        {
            var username = request.GetUserName();

            var customer = await _unitOfWork.Users.GetAll(false)
                .Where(q => q.Username == username)
                .Include(q => q.Customer).ThenInclude(q => q.Organizations)
                .Select(q => q.Customer).FirstOrDefaultAsync();

            if (customer == null)
            {
                return new OperationResult(result: false, statusCode: 400, message: $"No corresponding customer found for user: {request.GetUserName()}", value: null);
            }

            if (!customer.Organizations.Any())
            {
                return new OperationResult(true, 200, message: "No organizations defined for this user", value: null);
            }

            //Customer can see its own organizations only
            var customerValidOrganizations = customer.Organizations.Select(q => q.Id).Distinct().ToList();
            if (request.Filter.OrganizationIds == null || !request.Filter.OrganizationIds.Any())
            {
                request.Filter.OrganizationIds = customerValidOrganizations;
            }
            else
            {
                foreach (var orgId in request.Filter.OrganizationIds)
                {
                    if(!customerValidOrganizations.Any(q => q == orgId))
                    {
                        return new OperationResult(result: false, statusCode: 400, message: $"Access Denied to OrganizationId :{orgId}", value: null);
                    }
                }
            }            

            var entities = await _unitOfWork.Departments.GetAllByFilterAsync(request.Filter);
            if(!entities.Any())
            {
                return new OperationResult(true, 204, message: "No departments defined for this customer", value: null);
            }

            IList<DepartmentDto> departments = LoadDepartmentsStructure(entities);

            var result = new ListResult<DepartmentDto>
            {
                Page = request.Filter.Page,
                PageSize = request.Filter.PageSize,
                Data = departments
            };

            return new OperationResult(true, 200, message: "", value: result);
        }

        private IList<DepartmentDto> LoadDepartmentsStructure(ICollection<Department> entities, bool isRoot = true)
        {
            IList<DepartmentDto> departmentsDtoList = new List<DepartmentDto>();

            foreach (var departmentEntity in entities)
            {
                var departmentDto = new DepartmentDto()
                {
                    Id = departmentEntity.Id,
                    OrganizationId = departmentEntity.OrganizationId,
                    ParentDepartmentId = departmentEntity.ParentDepartmentId,
                    Title = departmentEntity.Title
                };
                
                if(!isRoot || (isRoot && departmentEntity.ParentDepartmentId == null))
                {
                    departmentsDtoList.Add(departmentDto);
                }

                if(departmentEntity.SubDepartments != null && departmentEntity.SubDepartments.Any())
                {
                    var subDepartments = LoadDepartmentsStructure(departmentEntity.SubDepartments, false);
                    departmentDto.SubDepartments = subDepartments;
                }
            }


            return departmentsDtoList;
        }
    }
}
