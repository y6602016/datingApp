import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { map } from "rxjs/operators";
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';


@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;

  // ReplaySubject here acts like a buffer with size 1, it stores user inside 
  private currentUserSource = new ReplaySubject<User>(1);

  // "$" at the end means it's an observable
  currentUser$ = this.currentUserSource.asObservable();
  

  constructor(private http: HttpClient) { }

  login(model: any) {
    // process the observable here, before it has been subscribed
    return this.http.post(this.baseUrl + 'account/login', model).pipe(
      map((response: User)=>{
        // process it as user
        const user = response;
        if (user) {
          this.setCurrentUser(user);
        }
      })
    )
  }

  // send post request to the server
  // when user register, we set his status login, so we use the same pipe as login
  register(model:any) {
    return this.http.post(this.baseUrl + 'account/register', model).pipe(
      map((user: User) => {
        if(user) {
          this.setCurrentUser(user);
        }
        // if we need the observable information, remeber to return in operator!!
        // or it return nothing
        
        // return user;
      })
    )
  }

  setCurrentUser(user: User) {
    // key = 'user', value = stringfied response
    // store it into the browser to achieve persisted login
    localStorage.setItem('user', JSON.stringify(user));

    // set currentUser as the user, which means we use the user to replace the currentuser
    this.currentUserSource.next(user);
  }

  logout() {
    // remove user key from browser if user log out
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }
}
