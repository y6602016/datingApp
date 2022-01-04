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

  constructor(private accountService: AccountService) {}

  ngOnInit(): void {
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

}
