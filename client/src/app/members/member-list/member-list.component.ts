import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { User } from 'src/app/_models/user';
import { UserParams } from 'src/app/_models/userParams';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  // memberService.getMembers() return observable, so type of members is Observable<Member[]>
  // members$: Observable<Member[]>;

  // due to pagination, set default ppageNumber and pageSize here
  members: Member[];
  pagination: Pagination;
  userParams: UserParams;
  user: User;
  genderList = [{value: 'male', display: 'Males'}, {value:'female', display: 'Females'}];

  constructor(private memberService: MembersService, private accountService: AccountService) { 
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UserParams(user);
    }) 
  }

  ngOnInit(): void {
    // this.members$ = this.memberService.getMembers();
    this.loadMembers();
  }

  loadMembers() {
    // send http request then assign the paginated contect to the variable for front-end use
    this.memberService.getMembers(this.userParams).subscribe(response => {
      this.members = response.result;
      this.pagination = response.pagination;
    })
  }

  resetFilters() {
    this.userParams = new UserParams(this.user);
    this.loadMembers();
  }

  pageChanged(event: any) {
    this.userParams.pageNumber = event.page;
    this.loadMembers();
  }
}
