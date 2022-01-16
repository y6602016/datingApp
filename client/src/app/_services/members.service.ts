import { HttpClient} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  
  // Services are singleton, and a component needs the service, 
  // the service stay alive until the application close. So it's ideal to store member status in service.
  // we store members list in service to avoid reload members again and again, just like we "cache" members here
  members: Member[] = [];

  constructor(private http: HttpClient) { }
  
  // request header information is added and processed by JwtInterceptor, it will add header
  // with user token to request then send the request to backend
  getMembers(){
    // if we have members, we return the members from the service as an observable
    if (this.members.length > 0) {
      // "of" means something of observable
      return of(this.members);
    }

    // otherwise we call api and use operator map to process the observable to assign members to this.members
    return this.http.get<Member[]>(this.baseUrl + 'users').pipe(
      map(members => {
        this.members = members;
        return members;
      })
    )
  }

  getMember(username : string) {
    const member = this.members.find(x => x.username === username);
    if (member !== undefined) {
      return of(member);
    }
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member) {
    // once we receive observable from put api, we extract the member's index and update members
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      })
    )
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  }
}
