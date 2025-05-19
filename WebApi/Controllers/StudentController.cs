using Domain.DTOs.StudentDTOs;
using Domain.Filters;
using Domain.Response;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentController(IStudentService studentService) : ControllerBase
{
    [HttpPost]
    public async Task<Response<GetStudentDTO>> CreateStudent(CreateStudentDTO createStudent)
    {
        return await studentService.CreateStudent(createStudent);
    }

    [HttpPut("{studentId}")]
    public async Task<Response<GetStudentDTO>> UpdateStudent(int studentId, CreateStudentDTO updateStudent)
    {
        return await studentService.UpdateStudent(studentId, updateStudent);
    }

    [HttpDelete("{studentId}")]
    public async Task<Response<string>> DeleteStudent(int studentId)
    {
        return await studentService.DeleteStudent(studentId);
    }

    [HttpGet("all")]
    public async Task<Response<List<GetStudentDTO>>> GetAllAsync([FromQuery] StudentFilter filter)
    {
        return await studentService.GetAllAsync(filter);
    }
}