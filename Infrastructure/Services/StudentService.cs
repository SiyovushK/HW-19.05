using System.Net;
using AutoMapper;
using Domain.DTOs.StudentDTOs;
using Domain.Entities;
using Domain.Filters;
using Domain.Response;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class StudentService(
        IBaseRepository<Student, int> studentRepository,
        IMapper mapper,
        ILogger<StudentService> logger) : IStudentService
{
    public async Task<Response<GetStudentDTO>> CreateStudent(CreateStudentDTO createStudent)
    {
        try
        {
            logger.LogInformation("Creating new student: {FirstName} {LastName}", createStudent.FirstName, createStudent.LastName);
    
            var student = mapper.Map<Student>(createStudent);
    
            var result = await studentRepository.AddAsync(student);
    
            if (result == 0)
            {
                logger.LogWarning("Failed to create student");
                return new Response<GetStudentDTO>(HttpStatusCode.BadRequest, "Student not created");
            }
    
            var getStudentDto = mapper.Map<GetStudentDTO>(student);
    
            logger.LogInformation("Student created successfully with ID {Id}", student.StudentId);
            return new Response<GetStudentDTO>(getStudentDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating student");
            return new Response<GetStudentDTO>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<GetStudentDTO>> UpdateStudent(int studentId, CreateStudentDTO updateStudent)
    {
        try
        {
            logger.LogInformation("Updating student: {Id} {FirstName} {LastName}", studentId, updateStudent.FirstName, updateStudent.LastName);

            var student = await studentRepository.GetByIdAsync(studentId);
            if (student == null)
                return new Response<GetStudentDTO>(HttpStatusCode.NotFound, "Student is not found");
            
            student.FirstName = updateStudent.FirstName;
            student.LastName = updateStudent.LastName;
            student.BirthDate = updateStudent.BirthDate;
    
            var result = await studentRepository.UpdateAsync(student);
    
            if (result == 0)
            {
                logger.LogWarning("Failed to update student");
                return new Response<GetStudentDTO>(HttpStatusCode.BadRequest, "Student not updated");
            }

            var getStudentDto = mapper.Map<GetStudentDTO>(student);
    
            logger.LogInformation("Student updated successfully");
            return new Response<GetStudentDTO>(getStudentDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating student");
            return new Response<GetStudentDTO>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<string>> DeleteStudent(int studentId)
    {
        try
        {
            logger.LogInformation("Deleting student: {Id}", studentId);

            var student = await studentRepository.GetByIdAsync(studentId);
            if (student == null)
                return new Response<string>(HttpStatusCode.NotFound, "Student is not found");
    
            var result = await studentRepository.DeleteAsync(student);

            if (result == 0)
            {
                logger.LogWarning("Failed to delete student");
                return new Response<string>(HttpStatusCode.BadRequest, "Student not deleted");
            }
    
            logger.LogInformation("Student deleted successfully with ID {Id}", studentId);
            return new Response<string>("Student deleted successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting student");
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<List<GetStudentDTO>>> GetAllAsync(StudentFilter filter)
    {
        try
        {
            logger.LogInformation("Getting students with filter: {@Filter}", filter);

            var pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 10 ? 10 : filter.PageSize;
    
            var studentQuery = await studentRepository.GetAllAsync();
    
            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var nameFilter = filter.Name.ToLower();
                studentQuery = studentQuery.Where(s => 
                    (s.FirstName + " " + s.LastName).ToLower().Contains(nameFilter));
            }
    
            if (filter.From != null)
            {
                var year = DateTime.UtcNow.Year;
                studentQuery = studentQuery.Where(s => year - s.BirthDate.Year >= filter.From);
            }
    
            if (filter.To != null)
            {
                var year = DateTime.UtcNow.Year;
                studentQuery = studentQuery.Where(s => year - s.BirthDate.Year <= filter.To);
            }
    
            var totalRecords = await studentQuery.CountAsync();
    
            var student = await studentQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
    
            var studentDtos = mapper.Map<List<GetStudentDTO>>(student);
    
            logger.LogInformation("Retrieved {Count} students", studentDtos.Count);
            return new PagedResponse<List<GetStudentDTO>>(studentDtos, pageNumber, pageSize, totalRecords);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting student");
            return new Response<List<GetStudentDTO>>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}