// import { useAppSelector } from '@/app/store';
// import { selectCurrentUser } from '@/app/store/authSlice';
// import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
// import { Avatar, AvatarFallback, AvatarImage } from '@/shared/ui/avatar';
// import { Button } from '@/shared/ui/button';
// import { Badge } from '@/shared/ui/badge';
// import { useNavigate } from 'react-router-dom';
// import { useAppDispatch } from '@/app/store';
// import { logout } from '@/app/store/authSlice';

// export const ProfilePage = () => {
//   const user = useAppSelector(selectCurrentUser);
//   const dispatch = useAppDispatch();
//   const navigate = useNavigate();

//   const handleLogout = () => {
//     dispatch(logout());
//     navigate('/auth/login');
//   };

//   if (!user) {
//     return (
//       <div className="flex items-center justify-center min-h-[60vh]">
//         <p>You need to be logged in to view this page.</p>
//       </div>
//     );
//   }

//   return (
//     <div className="container mx-auto py-6 max-w-3xl">
//       <Card>
//         <CardHeader className="flex flex-col items-center space-y-4">
//           <Avatar className="w-24 h-24">
//             <AvatarImage src={user.avatar} alt={user.name} />
//             <AvatarFallback>{user.name.charAt(0)}</AvatarFallback>
//           </Avatar>
//           <div className="text-center">
//             <CardTitle className="text-2xl">{user.name}</CardTitle>
//             <CardDescription>{user.email}</CardDescription>
//             <Badge variant="secondary" className="mt-2">
//               {user.role.toUpperCase()}
//             </Badge>
//           </div>
//         </CardHeader>
//         <CardContent>
//           <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
//             <div>
//               <h3 className="font-medium text-muted-foreground">User ID</h3>
//               <p>{user.id}</p>
//             </div>
//             <div>
//               <h3 className="font-medium text-muted-foreground">Member Since</h3>
//               <p>{new Date(user.createdAt).toLocaleDateString()}</p>
//             </div>
//           </div>

//           <div className="flex flex-col sm:flex-row gap-3">
//             <Button onClick={() => navigate('/settings')}>
//               Edit Profile
//             </Button>
//             <Button variant="outline" onClick={handleLogout}>
//               Logout
//             </Button>
//           </div>
//         </CardContent>
//       </Card>
//     </div>
//   );
// };

// export default ProfilePage;