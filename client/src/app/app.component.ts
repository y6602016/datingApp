import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'Dating app';
  users : any;

  // we won't make http request in constructor, it's too early
  // so we use OnInit lifecycle hook to make http request
  constructor(private http: HttpClient, private accountService: AccountService) {}

  ngOnInit(): void {
    // it's better to call method here and implement http request in method
    this.getUsers();
    // to persist user login, we check browser contains a user's key or not
    // so we call setCurrentUser()
    this.setCurrentUser();
  }

  // used to find the user stringify observable from browser(stored in account.service file)
  // then set it as current user
  setCurrentUser() {
    const user: User = JSON.parse(localStorage.getItem('user'));
    // if not found, it pass user as null.
    this.accountService.setCurrentUser(user);
  }

  getUsers() {
    // http request return observable, and observable is lazy
    // we need to call subscribe to operate the data
    this.http.get('https://localhost:5001/api/users').subscribe(response => {
      // response contains users
      this.users = response;
    }, error => {
      console.log(error);
    })
  }
}
