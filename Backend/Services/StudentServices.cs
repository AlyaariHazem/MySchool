using Backend.Data;
using Backend.DTOS;
using Backend.DTOS.GuardiansDTO;
using Backend.DTOS.RegisterStudentsDTO;
using Backend.DTOS.StudentsDTO;
using Backend.Models;
using Backend.Repository.IRepository;
using Backend.Repository.School.Interfaces;
using Backend.Services;
using Backend.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
namespace Backend.Services
{
    public class StudentServices : IStudentServices
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IGuardianServices _guardianServices;
        private readonly IUserServices _userServices;
        public StudentServices(IStudentRepository studentRepository,
         IGuardianServices guardianServices, IUserServices userServices)
        {
            _guardianServices = guardianServices; ;
            _studentRepository = studentRepository;
            _userServices = userServices;
        }
        public async Task<Student> AddAsync(Student student)
        {
            await _studentRepository.CreateAsync(student);

            return student;
        }

        public async Task<StudentDetailsDTO?> GetWithDetailsAsync(Expression<Func<Student, bool>> fillter)
        {
            var student = await _studentRepository.GetAsync(fillter, includeProperties: "Division,ApplicationUser");

            if (student == null)
            {
                return null;
            }

            string baseUrl = "https://localhost:7258/uploads/StudentPhotos";

            return new StudentDetailsDTO
            {
                StudentID = student.StudentID,
                FullName = new NameDTO
                {
                    FirstName = student.FullName.FirstName,
                    MiddleName = student.FullName.MiddleName,
                    LastName = student.FullName.LastName
                },
                PhotoUrl = student.ImageURL != null
                    ? $"{baseUrl}/{student.ImageURL}"
                    : $"{baseUrl}/default-placeholder.png",
                DivisionID = student.DivisionID,
                PlaceBirth = student.PlaceBirth,
                UserID = student.UserID,
                ApplicationUser = new ApplicationUserDTO
                {
                    Id = student.ApplicationUser.Id,
                    UserName = student.ApplicationUser.UserName!,
                    Email = student.ApplicationUser.Email!,
                    Gender = student.ApplicationUser.Gender
                }
            };
        }
        public async Task<Student?> GetAsync(Expression<Func<Student, bool>> fillter)
        {
            var student = await _studentRepository.GetAsync(fillter, includeProperties: "Division,ApplicationUser");

            if (student == null)
            {
                return null;
            }
            return student;

        }

        public async Task<List<StudentDetailsDTO>> GetAllWithDetailsAsync(Expression<Func<Student, bool>> fillter = null)
        {
            var students = await _studentRepository.GetAllAsync(fillter, includeProperties: "ApplicationUser,Division.Class.Stage,AccountStudentGuardians,Guardian");


            if (students == null || !students.Any())
                return new List<StudentDetailsDTO>();

            string baseUrl = "https://localhost:7258/uploads/StudentPhotos";

            return students.Select(student => new StudentDetailsDTO
            {
                StudentID = student.StudentID,
                FullName = new NameDTO
                {
                    FirstName = student.FullName.FirstName,
                    MiddleName = student.FullName.MiddleName,
                    LastName = student.FullName.LastName
                },
                PhotoUrl = student.ImageURL != null
                    ? $"{baseUrl}/{student.ImageURL}"
                    : $"{baseUrl}/default-placeholder.png",
                DivisionID = student.DivisionID,
                DivisionName = student.Division?.DivisionName,
                ClassName = student.Division?.Class?.ClassName,
                StageName = student.Division?.Class?.Stage?.StageName,
                Age = student.StudentDOB.HasValue
                    ? DateTime.Now.Year - student.StudentDOB.Value.Year
                    : (int?)null,
                Gender = student.ApplicationUser.Gender,
                HireDate = student.ApplicationUser.HireDate,
                PlaceBirth = student.PlaceBirth,
                Fee = student.AccountStudentGuardians?.Sum(asg => asg.Amount) ?? 0, // Aggregate Fee Amount
                StudentPhone = student.ApplicationUser.PhoneNumber,
                StudentAddress = student.ApplicationUser.Address,
                UserID = student.UserID,
                ApplicationUser = new ApplicationUserDTO
                {
                    Id = student.ApplicationUser.Id,
                    UserName = student.ApplicationUser.UserName!,
                    Email = student.ApplicationUser.Email!,
                    Gender = student.ApplicationUser.Gender
                },
                Guardians = new GuardianDto
                {
                    guardianFullName = student.Guardian.FullName,
                    guardianType = student.Guardian.Type!,
                    guardianEmail = student.ApplicationUser.Email,
                    guardianPhone = student.ApplicationUser.PhoneNumber!,
                    guardianDOB = student.ApplicationUser.HireDate,
                    guardianAddress = student.ApplicationUser.Address!
                }
            }).ToList();
        }
        public async Task<RegisterResult> RegisterWithGuardianAsync(RegisterStudentWithGuardianDTO model)
        {
            RegisterResult result = new();
            if (!await _userServices.IsUinque(model.GuardianEmail))
            {

                result.IsSuccess = true;
                result.Error = "Thes Guardian Is already";
                return result;
            }
            if (!await _userServices.IsUinque(model.StudentEmail))
            {

                result.IsSuccess = true;
                result.Error = "Thes Student  Is already";
                return result;
            }
            using (var transaction = await _studentRepository.BeginTransactionAsync())
            {
                try
                {

                    var GuardenUser = new ApplicationUser()
                    {
                        Email = model.GuardianEmail,
                        UserName = model.GuardianPhone,
                        PhoneNumber = model.GuardianPhone,
                        Address = model.GuardianAddress,
                        UserType = "GUARDIAN"


                    };
                    GuardenUser = await _userServices.AddAsync(GuardenUser, model.GuardianPassword, "GUARDIAN");
                    if (GuardenUser == null)
                    {
                        return result;
                    }

                    var Guardian = new Guardian()
                    {
                        FullName = model.GuardianFullName,
                        GuardianDOB = model.GuardianDOB,
                        UserID = GuardenUser.Id,
                        Type = model.GuardianType

                    };
                    Guardian = await _guardianServices.AddAsync(Guardian);
                    model.GuardianId = Guardian.GuardianID;

                    await transaction.CommitAsync();

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    result.IsSuccess = false;
                    result.Error = ex.Message;
                    return result;
                }
            }

            var RegisterStuden = new RegisterStudentDTO()
            {
                StudentEmail = model.StudentEmail,
                StudentFirstName = model.StudentFirstName,
                StudentMiddleName = model.StudentMiddleName,
                StudentLastName = model.StudentLastName,
                StudentDOB = model.StudentDOB,
                StudentPhone = model.StudentPhone,
                StudentAddress = model.StudentAddress,
                DivisionID = model.DivisionID,
                GuardianID = model.GuardianId,
                PlaceBirth = model.PlaceBirth,
                StudentFirstNameEng = model.StudentFirstNameEng,
                StudentMiddleNameEng = model.StudentMiddleNameEng,
                StudentLastNameEng = model.StudentLastNameEng,
                StudentImageURL = model.StudentImageURL,
                StudentPassword = model.StudentPassword

            };
            result = await this.RegisterAsync(RegisterStuden);
            result.IsSuccess = true;
            return result;

        }
        public async Task<RegisterResult> RegisterAsync(RegisterStudentDTO model)
        {
            RegisterResult result = new();
            if (!await _userServices.IsUinque(model.StudentEmail))
            {
                result.IsSuccess = false;
                result.Error = "This Student is already registered";
                return result;
            }
            using (var transaction = await _studentRepository.BeginTransactionAsync())
            {
                try
                {
                    var studentUser = new ApplicationUser()
                    {
                        Email = model.StudentEmail,
                        UserName = model.StudentPhone,
                        PhoneNumber = model.StudentPhone,
                        Address = model.StudentAddress,
                        UserType = "STUDENT"
                    };

                    studentUser = await _userServices.AddAsync(studentUser, model.StudentPassword, "STUDENT");
                    if (studentUser == null)
                    {
                        result.IsSuccess = false;
                        result.Error = "Failed to create student user";
                        return result;
                    }

                    var student = new Student()
                    {
                        FullName = new Name
                        {
                            FirstName = model.StudentFirstName,
                            MiddleName = model.StudentMiddleName,
                            LastName = model.StudentLastName
                        },
                        FullNameAlis = new NameAlis
                        {
                            FirstNameEng = model.StudentFirstNameEng,
                            MiddleNameEng = model.StudentMiddleNameEng,
                            LastNameEng = model.StudentLastNameEng
                        },
                        StudentDOB = model.StudentDOB,
                        PlaceBirth = model.PlaceBirth,
                        DivisionID = model.DivisionID,
                        UserID = studentUser.Id,
                        GuardianID = model.GuardianID,
                        ImageURL = model.StudentImageURL
                    };

                    await _studentRepository.CreateAsync(student);
                    result.IsSuccess = true;
                    result.StudentId = student.StudentID;
                    await transaction.CommitAsync();
                    return result;



                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    result.IsSuccess = false;
                    result.Error = ex.Message;
                    return result;
                }
            }

        }
        public async Task<bool> DeleteAsync(int Id)
        {
            var Student = await _studentRepository.GetAsync(s => s.StudentID == Id);
            if (Student != null)
            {
                await _studentRepository.RemoveAsync(Student);
                return true;
            }
            return false;
        }
    }

}
