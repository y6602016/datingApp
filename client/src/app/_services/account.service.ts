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
    user.roles = [];
    // decode the user token and extract the token role payload
    const roles = this.getDecodedToken(user.token).role;
    // check roles is a array or not. if user only has one role, it's just a string not an array
    Array.isArray(roles) ? user.roles = roles : user.roles.push(roles);

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

  getDecodedToken(token) {
    return JSON.parse(atob(token.split('.')[1]));
  }
}
