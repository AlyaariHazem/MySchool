export interface RegisterStudent {
    UserName: string;
    UserType: string;
    Password: string;
    FirstName: string;
    SecondName: string;
    ThirdName: string;
    LastName: string;
    DateOfBirth: Date;
    Gender: string;
    Phone: string;
    GuardianID: number;
    DivisionID: number;
    CreatedAt: Date;
    UpdatedAt: Date;
  }
  
  export interface UserForLoginDto {
    UserName: string;
    Password: string;
    UserType: string;
  }
  