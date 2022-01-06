import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {}

  // we set it public since template file directly use accountService to access
  // currentUser$, template file will auto sub/unsub the obervables
  constructor(public accountService: AccountService, private router: Router) { }

  // to pwersist user after logging, we monitor currentUser observable in account service
  ngOnInit(): void {

  }

  login() {
    // service's login method contains http post method, it returns a observable
    // it's lazy, so we use subscribe. the response will be the UserDto
    this.accountService.login(this.model).subscribe(response => {
      this.router.navigateByUrl('/members');
    }, error=> { // catch error here, ex: invalid username, which is defined in AccountController
      console.log(error);
    })
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }

}
