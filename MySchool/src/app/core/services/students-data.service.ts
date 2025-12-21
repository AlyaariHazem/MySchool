import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { StudentDetailsDTO } from '../models/students.model';

export interface TableFilter {
  field: string;
  value: string;
}

@Injectable({
  providedIn: 'root'
})
export class StudentsDataService {
  // BehaviorSubject to hold the current students data
  private studentsSubject = new BehaviorSubject<StudentDetailsDTO[]>([]);
  public students$ = this.studentsSubject.asObservable();

  // Subject for filtered students
  private filteredStudentsSubject = new BehaviorSubject<StudentDetailsDTO[]>([]);
  public filteredStudents$ = this.filteredStudentsSubject.asObservable();

  // Subject for filters
  private filtersSubject = new BehaviorSubject<Record<string, string>>({});
  public filters$ = this.filtersSubject.asObservable();

  // Subject for loading state
  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  // Subject for pagination
  private paginationSubject = new BehaviorSubject<{
    currentPage: number;
    pageSize: number;
    totalRecords: number;
  }>({
    currentPage: 1,
    pageSize: 8,
    totalRecords: 0
  });
  public pagination$ = this.paginationSubject.asObservable();

  constructor() {}

  // Set students data
  setStudents(students: StudentDetailsDTO[]): void {
    this.studentsSubject.next(students);
    // Only apply filters if there are active filters
    const hasFilters = Object.keys(this.filtersSubject.value).length > 0;
    if (hasFilters) {
      this.applyFilters();
    } else {
      // No filters: just set filtered students to the same as students
      this.filteredStudentsSubject.next(students);
    }
  }

  // Add a single student
  addStudent(student: StudentDetailsDTO): void {
    const current = this.studentsSubject.value;
    this.setStudents([...current, student]);
  }

  // Update a student
  updateStudent(updatedStudent: StudentDetailsDTO): void {
    const current = this.studentsSubject.value;
    const updated = current.map(s =>
      s.studentID === updatedStudent.studentID ? updatedStudent : s
    );
    this.setStudents(updated);
  }

  // Delete a student
  deleteStudent(studentId: number): void {
    const current = this.studentsSubject.value;
    const filtered = current.filter(s => s.studentID !== studentId);
    this.setStudents(filtered);
  }

  // Set filter
  setFilter(field: string, value: string): void {
    const currentFilters = this.filtersSubject.value;
    if (value && value.trim() !== '') {
      this.filtersSubject.next({ ...currentFilters, [field]: value });
    } else {
      const { [field]: removed, ...rest } = currentFilters;
      this.filtersSubject.next(rest);
    }
    this.applyFilters();
  }

  // Clear all filters
  clearFilters(): void {
    this.filtersSubject.next({});
    this.applyFilters();
  }

  // Apply filters to students
  private applyFilters(): void {
    const students = this.studentsSubject.value;
    const filters = this.filtersSubject.value;

    if (!filters || Object.keys(filters).length === 0) {
      this.filteredStudentsSubject.next(students);
      return;
    }

    const filtered = students.filter(student => {
      return Object.entries(filters).every(([field, value]) => {
        if (!value || value.trim() === '') return true;

        const searchValue = value.toLowerCase().trim();

        switch (field) {
          case 'studentID':
            return String(student.studentID).includes(searchValue);
          case 'fullName':
            const fullName = `${student.fullName?.firstName || ''} ${student.fullName?.middleName || ''} ${student.fullName?.lastName || ''}`.toLowerCase();
            return fullName.includes(searchValue);
          case 'stageName':
            return student.stageName?.toLowerCase().includes(searchValue) || false;
          case 'className':
            return student.className?.toLowerCase().includes(searchValue) || false;
          case 'divisionName':
            return student.divisionName?.toLowerCase().includes(searchValue) || false;
          case 'gender':
            return student.gender?.toLowerCase().includes(searchValue) || false;
          case 'age':
            return String(student.age || '').includes(searchValue);
          case 'hireDate':
            if (student.hireDate) {
              const dateStr = new Date(student.hireDate).toLocaleDateString('ar-EG');
              return dateStr.includes(searchValue);
            }
            return false;
          default:
            return true;
        }
      });
    });

    this.filteredStudentsSubject.next(filtered);
  }

  // Set loading state
  setLoading(loading: boolean): void {
    this.loadingSubject.next(loading);
  }

  // Set pagination
  setPagination(pagination: { currentPage: number; pageSize: number; totalRecords: number }): void {
    this.paginationSubject.next(pagination);
  }

  // Get current values
  getCurrentStudents(): StudentDetailsDTO[] {
    return this.studentsSubject.value;
  }

  getCurrentFilteredStudents(): StudentDetailsDTO[] {
    return this.filteredStudentsSubject.value;
  }

  getCurrentFilters(): Record<string, string> {
    return this.filtersSubject.value;
  }
}

