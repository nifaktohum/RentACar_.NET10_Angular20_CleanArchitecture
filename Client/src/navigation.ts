export interface NavigationModel {
  title: string
  key: string       // ⭐ EKLE - Benzersiz anahtar (örn: 'vehicles', 'reservations')
  icon: string
  url?: string
  role: string
  haveSubMenu: boolean
  subMenus?: {
    title: string;
    url: string;
    icon?: string;
    role: string;
    haveSubMenu?: boolean;
  }[]
}
export const navigations: NavigationModel[] =
  [
    // Dashboard
    {
      title: 'Dashboard',
      key: 'dashboard',
      url: '/admin/dashboard',
      icon: 'ri-dashboard-line',
      haveSubMenu: false,
      role: 'Dashboard.Read'
    },
    // Vehicles
    {
      title: 'Araç Yönetimi',
      key: 'vehicles',
      url: '/admin/vehicles',
      icon: 'ri-car-line',
      role: 'Vehicles.Read',
      haveSubMenu: true, // Alt menüsü var dedik
      subMenus: [        // Alt menü elemanları yine birer NavigationModel'dir
        {
          title: 'Araç Listesi',
          url: '/admin/vehicles/list',
          icon: 'ri-list-check',
          role: 'Vehicles.Read',
          haveSubMenu: false
        },
        {
          title: 'Yeni Araç Ekle',
          url: '/admin/vehicles/add',
          icon: 'ri-add-circle-line',
          role: 'Vehicles.Create',
          haveSubMenu: false
        }
      ],

    },
    // Categories 
    {
      title: 'Kategoriler',
      key: 'categories',
      icon: 'ri-node-tree',
      role: 'Categories.Read',
      haveSubMenu: true,
      subMenus: [
        {
          title: 'Kategori Listesi',
          url: '/admin/categories',
          icon: 'ri-list-check',
          role: 'Categories.Read',
          haveSubMenu: false
        },
        {
          title: 'Kategori Ekle',
          url: '/admin/categories/category-create',
          icon: 'ri-add-circle-line',
          role: 'Categories.Create',
          haveSubMenu: false
        },
        {
          title: 'Kategori Hiyerarşisi',
          url: '/admin/categories/hierarchy',
          icon: 'ri-tree-line',
          role: 'Categories.Read',
          haveSubMenu: false
        }
      ],
    },
    // Reservations
    {
      title: 'Rezervasyonlar',
      key: 'reservations',
      icon: 'ri-calendar-check-line',
      role: 'Reservations.Read',
      haveSubMenu: true,
      subMenus: [
        { title: 'Tüm Rezervasyonlar', url: '/admin/reservations', role: 'Reservations.Read' },
        { title: 'Yeni Rezervasyon', url: '/admin/reservations/add', role: 'Reservations.Read' },
        { title: 'Aktif Rezervasyonlar', url: '/admin/reservations/active', role: 'Reservations.Read' }
      ],
    },
    // Customers
    {
      title: 'Müşteriler',
      key: 'customers',
      icon: 'ri-group-line',
      role: 'Customers.Read',
      haveSubMenu: true,
      subMenus: [
        { title: 'Müşteri Listesi', url: '/admin/customers', role: 'Customers.Read' },
        { title: 'Müşteri Ekle', url: '/admin/customers/add', role: 'Customers.Create' },
        { title: 'Kara Liste', url: '/admin/customers/blacklist', role: 'Customers.Read' }
      ],
    },
    // Branches
    {
      title: 'Şubeler',
      key: 'branches',
      url: '/admin/branches',
      icon: 'ri-map-pin-user-line',
      role: 'Branches.Read',
      haveSubMenu: false,
    },
    // Roles
    {
      title: 'Roller & Yetkiler',
      key: 'roles',
      icon: 'ri-key-2-line',
      url: '/admin/roles',
      role: 'Roles.Read',
      haveSubMenu: false,
    },
    // Users
    {
      title: 'Kullanıcılar',
      key: 'users',
      icon: 'ri-user-settings-line',
      role: 'Users.Read',
      haveSubMenu: true,
      subMenus: [
        { title: 'Kullanıcı Listesi', url: '/admin/users', role: 'Users.Read' },
        { title: 'Kullanıcı Ekle', url: '/admin/users/add', role: 'Users.Create' }
      ],
    }
  ];
