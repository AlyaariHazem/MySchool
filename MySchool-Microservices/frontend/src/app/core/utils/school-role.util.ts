/** True when the logged-in user is school-side HR (matches backend ADMIN,MANAGER recruitment rules). */
export function isSchoolHrManager(): boolean {
  if (typeof localStorage === 'undefined') {
    return false;
  }
  const t = localStorage.getItem('userType');
  return t === 'ADMIN' || t === 'MANAGER';
}

/** School portal manager (single school); not system ADMIN. */
export function isSchoolManagerUser(): boolean {
  if (typeof localStorage === 'undefined') {
    return false;
  }
  return localStorage.getItem('userType') === 'MANAGER';
}
