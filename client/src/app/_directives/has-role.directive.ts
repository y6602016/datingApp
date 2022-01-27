import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { take } from 'rxjs/operators';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Directive({
  selector: '[appHasRole]' // will be used like: *appHasRole='["Admin", "Moderator"]'
})
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string[]; // accomadate the format above example
  user: User;
  constructor(private viewContainerRef: ViewContainerRef, 
    private templateRef: TemplateRef<any>, 
    private accountService: AccountService) { 
      this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
        this.user = user;
      })
    }
  
    ngOnInit(): void {
      // clear view if no roles
      if (!this.user?.roles || this.user == null) {
        this.viewContainerRef.clear()
        return;
      }
 
      // if the user has "Admin" or "Moderator" role, create the EmbeddedView(in nav.component html file)
      if (this.user?.roles.some(r => this.appHasRole.includes(r))) {
        this.viewContainerRef.createEmbeddedView(this.templateRef);
      }
      else {
        this.viewContainerRef.clear();
      }
    }

}
