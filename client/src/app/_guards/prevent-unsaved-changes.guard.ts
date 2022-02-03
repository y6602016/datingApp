import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanDeactivate, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { MemberEditComponent } from '../members/member-edit/member-edit.component';
import { ConfirmService } from '../_services/confirm.service';

@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangesGuard implements CanDeactivate<unknown> {

  constructor(private confirmService: ConfirmService){}

  // if user has modified their profile unsaving it and exit the page, ask them to ensure data lost is ok
  canDeactivate(component: MemberEditComponent): Observable<boolean> | boolean{
    if (component.editForm.dirty) {
      return this.confirmService.confirm()
    }
    return true;
  }
  
}
