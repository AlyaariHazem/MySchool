/** True when a JWT exists (user is logged in). */
export function hasAuthToken(): boolean {
  return typeof localStorage !== 'undefined' && !!localStorage.getItem('token');
}
