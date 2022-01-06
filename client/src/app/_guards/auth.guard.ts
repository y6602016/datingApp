import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot, UrlTree } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AccountService } from '../_services/account.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  // inject accountService to get current user status
  constructor(private accountService : AccountService, private toastr: ToastrService){}

  canActivate(): Observable<boolean> {
    // get observable and use map operator to check user status
    return this.accountService.currentUser$.pipe(
      map(user => {
        if (user) {
          return true;
        }
        // if not return and go here, means current user is null
        this.toastr.error('You shall not pass!')
      })
    )
  }
  
}
