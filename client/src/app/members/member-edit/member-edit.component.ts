import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  // access editFrom variable in template file by @ViewChild, we access it to reset the updated profile status
  @ViewChild('editForm') editForm: NgForm;
  member: Member;
  user: User;
  // if user has unsaved chages, and they are exiting the edit page, we have 2 actions:
  // 1. if users are heading to other pages in the website, the PreventUnsavedChangesGuard in the routing will notice them
  // 2. if users are exiting the website, use HostListener to access the browser event, which is window:beforeunload, ['$event']
  // HostListener can notice user if they change dom event with unsaved changes
  @HostListener('window:beforeunload', ['$event']) unloadNotifitcation($event: any) {
    if (this.editForm.dirty) {
      $event.returnValue = true;
    }
  }

  constructor(private accountService: AccountService, private memberService: MembersService, private toastr: ToastrService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => this.user = user);  
  }

  ngOnInit(): void {
    this.loadMember();
  }
  
  loadMember() {
    this.memberService.getMember(this.user.username).subscribe(member => {
      this.member = member;
    })
  }

  updateMember() {
    console.log(this.member);
    this.toastr.success('Profile updated successfully')
    // we access "editFrom" variable to reset the updated profile status
    this.editForm.reset(this.member);
  }
}
