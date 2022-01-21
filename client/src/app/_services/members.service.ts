import { HttpClient, HttpParams} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, pipe } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  
  // Services are singleton, and a component needs the service, 
  // the service stay alive until the application close. So it's ideal to store member status in service.
  // we store members list in service to avoid reload members again and again, just like we "cache" members here
  members: Member[] = [];

  // cache to stroe {key = userParams, value = query objects}
  // so that every same params can store it's query in chache, no need to hit db again
  memberCache = new Map();
  user: User;
  userParams: UserParams;

  
  constructor(private http: HttpClient, private accountService: AccountService) {
    // store UserParams object in this service since service is a singleton
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UserParams(user);
    })
  }

  // three process to modify userParam object: get, set, reset
  getUserParams() {
    return this.userParams;
  }

  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  resetUserParams() {
    this.userParams = new UserParams(this.user);
    return this.userParams;
  }
  
  // request header information is added and processed by JwtInterceptor, it will add header
  // with user token to request then send the request to backend
  getMembers(userParams: UserParams){
    // check if we have cache for this userParams
    // Object.values(userParams).join('-') can be as the key
    var response = this.memberCache.get(Object.values(userParams).join('-'));
    if (response) {
      return of(response);
    }

    let params = this.getPaginationHeaders(userParams.pageNumber, userParams.pageSize)

    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);

    // we call api and use operator map to process the observable to assign members to this.members
    // we observe the response with the params we just created
    return this.getPaginatedResult<Member[]>(this.baseUrl + 'users', params)
      .pipe(map(response => {
        // set map's key and value pair as cache
        this.memberCache.set(Object.values(userParams).join('-'), response);
        return response;
      }))
  }


  private getPaginatedResult<T>(url: string, params: HttpParams) {
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();
    return this.http.get<T>(url, { observe: 'response', params }).pipe(
      map(response => {
        // type of this.paginatedResult.result is Member[], which is same as response.body
        paginatedResult.result = response.body;
        if (response.headers.get('Pagination') !== null) {
          // assign the response header's Pagination value assign to paginatedResult.pagination
          paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }
        return paginatedResult;
      })
    );
  }

  private getPaginationHeaders(pageNumber: number, pageSize: number) {
    let params = new HttpParams();

    // we process pagination, convert them to string then append to params
    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());

    return params;
  }

  getMember(username : string) {
    // process it as a cache, use member to access the quieried members, if the member
    // we are finding is already in memberCache, just retreive it.
    const member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.result), []) // flattent the member list
      .find((member: Member) => member.username === username); // find the first one matches
    
    if (member) {
      return of(member);
    }
    // legacy code since we are using cache
    // const member = this.members.find(x => x.username === username);
    // if (member !== undefined) {
    //   return of(member);
    // }
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
